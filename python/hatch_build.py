import sys
from os import path

# needed for the "renaming" package
sys.path.append(path.dirname(__file__))
from pathlib import Path
import tempfile
from typing import Iterator
import grpc_tools.protoc
from renaming.fdsetgen import CustomProtoc

from hatchling.builders.hooks.plugin.interface import BuildHookInterface

_SCRIPT_PATH = Path(__file__).absolute()
PROJECT_PATH = path.join(_SCRIPT_PATH.parent, "src", "ares_datamodel")
PROTO_DIR = path.join(_SCRIPT_PATH.parent.parent, "protos")
DATAMODEL_PATH = path.join(PROTO_DIR, "datamodel")
SERVICES_PATH = path.join(PROTO_DIR, "services")

class CustomBuildHook(BuildHookInterface):
    PLUGIN_NAME = "protogen"

    def initialize(self, version: str, build_data: dict[str, any]) -> None:
        grpc_tools_dir = path.dirname(grpc_tools.__file__)
        # The well-known types are in the included directory of the protobuf library
        # which is bundled with grpcio-tools
        proto_include_dir = path.join(grpc_tools_dir, "_proto")

        protoc_args = [
            "grpc_tools.protoc",
            f"--proto_path={DATAMODEL_PATH}",
            f"--proto_path={SERVICES_PATH}",
            f"--proto_path={proto_include_dir}",
            f"--python_out={PROJECT_PATH}",
            f"--pyi_out={PROJECT_PATH}",
            f"--grpc_python_out={PROJECT_PATH}",
        ]

        for proto_file in grab_protos(PROTO_DIR):
            proto_path_str = str(proto_file.absolute())

            with tempfile.NamedTemporaryFile(delete=False) as f:
              descriptor_path = Path(f.name)
              test = f"--descriptor_set_out={descriptor_path}"
              args_with_file = protoc_args + [test, proto_path_str]
              result = grpc_tools.protoc.main(args_with_file)

              # We need to fix the imports as the standard protoc compiler
              # does not do well with relative paths
              generator = CustomProtoc(descriptor_path)
              generator.fix_imports(python_out=Path(PROJECT_PATH), create_package=False, overwrite_callback=overwrite_callback, module_suffixes=["_pb2.py", "_pb2_grpc.py"], exclude_imports_glob=["google/protobuf/*"])

              if result == 0:
                  print(f"Generated proto from: {proto_path_str}")
              if result != 0:
                  raise RuntimeError(f"Failed to compile proto file {proto_path_str}")


def grab_protos(dir: str) -> Iterator[Path]:
    search_path = Path(dir)

    for file_path in search_path.rglob("*.proto"):
        yield file_path


def overwrite_callback(python_file: Path, new_code: str):
    try:
        with open(python_file, 'w') as file:
            file.write(new_code)

        print(f"File {python_file} updated")
    except FileNotFoundError:
        print(f"Error: The file '{python_file}' was not found.")

    except IOError as e:
        print(f"An I/O error occurred: {e}")