using System;
using System.IO;
using System.Text;
using WolvenKit.RED4.IO;
using WolvenKit.RED4.Types;
using WolvenKit.RED4.Types.Exceptions;

namespace WolvenKit.RED4.Archive.IO
{
    public partial class CR2WReader : Red4Reader
    {
        public CR2WReader(Stream input) : base(input)
        {
        }

        public CR2WReader(Stream input, Encoding encoding) : base(input, encoding)
        {
        }

        public CR2WReader(Stream input, Encoding encoding, bool leaveOpen) : base(input, encoding, leaveOpen)
        {
        }

        public CR2WReader(BinaryReader reader) : base(reader)
        {
        }

        public override void ReadClass(IRedClass cls, uint size)
        {
            if (cls is IRedCustomData customCls)
            {
                customCls.CustomRead(this, size);
                return;
            }

            var startpos = _reader.BaseStream.Position;

            #region initial checks

            // ... okay CDPR, is that a joke or what?
            int zero = _reader.ReadByte();
            if (zero != 0)
            {
                throw new Exception($"Tried parsing a CVariable: zero read {zero}.");
            }

            #endregion

            #region parse sequential variables
            while (true)
            {
                var cvar = ReadVariable(cls);
                if (!cvar)
                    break;
            }
            #endregion

            var endpos = _reader.BaseStream.Position;
            var bytesread = endpos - startpos;

            if (cls is IRedAppendix app)
            {
                app.Read(this, (uint)(size - bytesread));
            }

            if (bytesread != size)
            {
                //throw new InvalidParsingException($"Read bytes not equal to expected bytes. Difference: {bytesread - size}");
            }
        }

        public bool ReadVariable(IRedClass cls)
        {
            var nameId = _reader.ReadUInt16();
            if (nameId == 0)
            {
                return false;
            }
            var varname = GetStringValue(nameId);

            // Read Type
            var typeId = _reader.ReadUInt16();
            var typename = GetStringValue(typeId);

            // Read Size
            var sizepos = _reader.BaseStream.Position;
            var size = _reader.ReadUInt32();

            IRedType value;

            var prop = RedReflection.GetPropertyByRedName(cls.GetType(), varname);
            if (cls.GetType() == typeof(audioPlayerWeaponSettings) && varname == "animEventOverrides")
            {
                var i = prop.Ordinal;
            }

            if (prop == null)
            {
                var (type, flags) = RedReflection.GetCSTypeFromRedType(typename);
                value = Read(type, size - 4, flags);

                RedReflection.AddDynamicProperty(cls, varname, value);
                //throw new MissingRTTIException(varname, typename, cls.GetType().Name);
            }
            else
            {
                value = Read(prop.Type, size - 4, prop.Flags.Clone());
                //PostProcessing(value);

                var typeInfo = RedReflection.GetTypeInfo(cls.GetType());
                if (!typeInfo.SerializeDefault && RedReflection.IsDefault(cls.GetType(), varname, value))
                {
                    throw new TodoException();
                }

                prop.SetValue(cls, value);
            }

            return true;
        }
    }
}
