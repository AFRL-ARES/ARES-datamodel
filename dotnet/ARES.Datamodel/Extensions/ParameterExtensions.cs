using Ares.Datamodel;
using Ares.Datamodel.Templates;

namespace Ares.Datamodel.Extensions;

public static class ParameterExtensions
{
  public static ParameterSource GetParameterSource(this Parameter parameter)
    => parameter.SourceCase switch
    {
      Parameter.SourceOneofCase.PlannedSource => ParameterSource.Planned,
      Parameter.SourceOneofCase.EnvironmentSource => ParameterSource.Environment,
      Parameter.SourceOneofCase.LiteralSource => ParameterSource.Value,
      Parameter.SourceOneofCase.CommandVariableSource => ParameterSource.Variable,
      _ => ParameterSource.Unspecified
    };

  public static AresValue? GetValue(this Parameter parameter)
    => parameter.SourceCase switch
    {
      Parameter.SourceOneofCase.LiteralSource => parameter.LiteralSource.Value,
      Parameter.SourceOneofCase.PlannedSource => parameter.PlannedSource.Value,
      Parameter.SourceOneofCase.EnvironmentSource => parameter.EnvironmentSource.Value,
      Parameter.SourceOneofCase.CommandVariableSource => parameter.CommandVariableSource.Value,
      _ => null
    };

  public static void SetLiteralSource(this Parameter parameter, AresValue? value)
    => parameter.LiteralSource = new LiteralParameterSource { Value = value ?? new AresValue() };

  public static void SetResolvedValue(this Parameter parameter, AresValue? value)
  {
    switch(parameter.SourceCase)
    {
      case Parameter.SourceOneofCase.PlannedSource:
        parameter.PlannedSource.Value = value ?? new AresValue();
        break;

      case Parameter.SourceOneofCase.EnvironmentSource:
        parameter.EnvironmentSource.Value = value ?? new AresValue();
        break;

      case Parameter.SourceOneofCase.CommandVariableSource:
        parameter.CommandVariableSource.Value = value ?? new AresValue();
        break;

      default:
        parameter.SetLiteralSource(value);
        break;
    }
  }

  public static bool IsPlanned(this Parameter parameter)
    => parameter.SourceCase == Parameter.SourceOneofCase.PlannedSource;

  public static ParameterMetadata? GetPlanningMetadata(this Parameter parameter)
    => parameter.PlannedSource?.PlanningMetadata;

  public static void SetPlannedSource(this Parameter parameter, ParameterMetadata? planningMetadata)
    => parameter.PlannedSource = new PlannedParameterSource { PlanningMetadata = planningMetadata };

  public static bool IsEnvironmentBased(this Parameter parameter)
    => parameter.SourceCase == Parameter.SourceOneofCase.EnvironmentSource;

  public static VariableType GetVariableType(this Parameter parameter)
    => parameter.EnvironmentSource?.VariableType ?? VariableType.VarUnspecified;

  public static string GetVariableArgument(this Parameter parameter)
    => parameter.SourceCase switch
    {
      Parameter.SourceOneofCase.EnvironmentSource => parameter.EnvironmentSource.VariableArgument,
      Parameter.SourceOneofCase.CommandVariableSource => parameter.CommandVariableSource.VariableArgument,
      _ => string.Empty
    };

  public static void SetEnvironmentSource(this Parameter parameter, VariableType variableType, string variableArgument)
    => parameter.EnvironmentSource = new EnvironmentParameterSource
    {
      VariableType = variableType,
      VariableArgument = variableArgument
    };

  public static void SetCommandVariableSource(this Parameter parameter, string variableArgument)
    => parameter.CommandVariableSource = new CommandVariableParameterSource { VariableArgument = variableArgument };
}
