using static WolvenKit.RED4.Types.Enums;

namespace WolvenKit.RED4.Types
{
	public partial class LibTreeDefTreeVariableVector : LibTreeDefTreeVariable
	{
		[Ordinal(2)] 
		[RED("exportAsProperty")] 
		public CBool ExportAsProperty
		{
			get => GetPropertyValue<CBool>();
			set => SetPropertyValue<CBool>(value);
		}

		[Ordinal(3)] 
		[RED("defaultValue")] 
		public Vector3 DefaultValue
		{
			get => GetPropertyValue<Vector3>();
			set => SetPropertyValue<Vector3>(value);
		}

		public LibTreeDefTreeVariableVector()
		{
			Id = 65535;
			ReadableName = "TreeVar";
			ExportAsProperty = true;
			DefaultValue = new();

			PostConstruct();
		}

		partial void PostConstruct();
	}
}
