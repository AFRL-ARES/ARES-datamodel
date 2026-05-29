using System.Text.Json.Serialization;

namespace Ares.Datamodel.Templates;

public sealed partial class Parameter
{
  [JsonIgnore]
  public ParameterSourceJson? SourceJson
  {
    get => SourceCase switch
    {
      SourceOneofCase.LiteralSource => new ParameterSourceJson { LiteralSource = LiteralSource },
      SourceOneofCase.PlannedSource => new ParameterSourceJson { PlannedSource = PlannedSource },
      SourceOneofCase.EnvironmentSource => new ParameterSourceJson { EnvironmentSource = EnvironmentSource },
      SourceOneofCase.CommandVariableSource => new ParameterSourceJson { CommandVariableSource = CommandVariableSource },
      _ => null
    };
    set
    {
      ClearSource();
      if(value is null)
        return;

      if(value.LiteralSource is not null)
        LiteralSource = value.LiteralSource;

      else if(value.PlannedSource is not null)
        PlannedSource = value.PlannedSource;

      else if(value.EnvironmentSource is not null)
        EnvironmentSource = value.EnvironmentSource;

      else if(value.CommandVariableSource is not null)
        CommandVariableSource = value.CommandVariableSource;
    }
  }
}

public sealed class ParameterSourceJson
{
  public LiteralParameterSource? LiteralSource { get; set; }
  public PlannedParameterSource? PlannedSource { get; set; }
  public EnvironmentParameterSource? EnvironmentSource { get; set; }
  public CommandVariableParameterSource? CommandVariableSource { get; set; }
}
