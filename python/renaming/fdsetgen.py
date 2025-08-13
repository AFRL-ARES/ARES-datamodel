from __future__ import annotations

import abc
import fnmatch
import itertools
import re
from pathlib import Path
from typing import TYPE_CHECKING, Callable

from google.protobuf.descriptor_pb2 import FileDescriptorSet

from .rewrite import ASTImportRewriter, build_rewrites

if TYPE_CHECKING:
    from collections.abc import Iterable, Sequence

_PROTO_SUFFIX_PATTERN = re.compile(r"^(.+)\.proto$")


def _clean_proto_filename(name: str) -> str:
    """Remove the `.proto` suffix from `name`.

    Examples
    --------
    >>> _clean_proto_filename("a/b.proto")
    'a/b'
    >>> _clean_proto_filename("a/b-c.proto")
    'a/b_c'
    >>> _clean_proto_filename("a/b_c.proto")
    'a/b_c'
    """
    return _PROTO_SUFFIX_PATTERN.sub(r"\1", name).replace("-", "_")


def _should_ignore(fd_name: str, patterns: Sequence[str]) -> bool:
    """Return whether `fd_name` should be ignored according to `patterns`.

    Examples
    --------
    >>> fd_name = "google/protobuf/empty.proto"
    >>> pattern = "google/protobuf/*"
    >>> _should_ignore(fd_name, [pattern])
    True
    >>> fd_name = "foo/bar"
    >>> _should_ignore(fd_name, [pattern])
    False
    """
    return any(fnmatch.fnmatchcase(fd_name, pattern) for pattern in patterns)


class FileDescriptorSetGenerator(abc.ABC):
    """Base class that implements fixing imports."""

    @abc.abstractmethod
    def generate_file_descriptor_set_bytes(self) -> bytes:
        """Generate the bytes of a `FileDescriptorSet`."""

    def fix_imports(
        self,
        *,
        python_out: Path,
        create_package: bool,
        overwrite_callback: Callable[[Path, str], None],
        module_suffixes: Sequence[str],
        exclude_imports_glob: Sequence[str],
    ) -> None:
        """Fix imports from protoc/buf generated code."""
        fdset = FileDescriptorSet.FromString(self.generate_file_descriptor_set_bytes())

        has_pyi = any(suffix.endswith(".pyi") for suffix in module_suffixes)
        for fd in fdset.file:
            if _should_ignore(fd.name, exclude_imports_glob):
                continue

            fd_name = _clean_proto_filename(fd.name)
            rewriter = ASTImportRewriter()
            # services live outside of the corresponding generated Python
            # module, but they import it so we register a rewrite for the
            # current proto as a dependency of itself to handle the case
            # of services
            for repl in build_rewrites(fd_name, fd_name, is_public=False):
                rewriter.register_rewrite(repl)

            # register proto import rewrites

            # construct a frozenset for dependencies to check whether need to
            # rewrite using star imports
            public_deps = frozenset(fd.public_dependency)

            for i, dep in enumerate(map(_clean_proto_filename, fd.dependency)):
                if _should_ignore(dep, exclude_imports_glob):
                    continue

                dep_name = _clean_proto_filename(dep)
                for repl in build_rewrites(
                    fd_name, dep_name, is_public=i in public_deps
                ):
                    rewriter.register_rewrite(repl)

            for suffix in module_suffixes:
                python_file = python_out.joinpath(f"{fd_name}{suffix}")
                try:
                    raw_code = python_file.read_text()
                except FileNotFoundError:
                    pass
                else:
                    new_code = rewriter.rewrite(raw_code)
                    overwrite_callback(python_file, new_code)

        if create_package:
            # recursively create packages
            for dir_entry in itertools.chain([python_out], python_out.rglob("*")):
                if dir_entry.is_dir() and "__pycache__" not in dir_entry.parts:
                    dir_entry.joinpath("__init__.py").touch(exist_ok=True)
                    if has_pyi:
                        _create_pyi_init(dir_entry)


def _create_pyi_init(root: Path) -> None:
    # use a dictionary to preserve order while deduplicating
    lines_to_write = {
        f"from . import {path.stem}\n": None
        for path in sorted(root.glob("*"))
        if path.stem not in ("__init__", "__pycache__")
        if path.suffix == ".pyi" or path.is_dir()
    }
    path = root.joinpath("__init__.pyi")

    if not path.exists():
        path.write_text("".join(lines_to_write))
    else:
        with path.open(mode="r") as f:
            for line in f:
                lines_to_write.pop(line, None)
        with path.open(mode="a") as f:
            f.writelines(lines_to_write)


# Custom generator that works with protoletariat
class CustomProtoc(FileDescriptorSetGenerator):
    def __init__(
        self,
        descriptor_path: Path
    ) -> None:
        self.descriptor_path = descriptor_path

    def generate_file_descriptor_set_bytes(self) -> bytes:
        return self.descriptor_path.read_bytes()
