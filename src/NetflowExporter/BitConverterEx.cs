using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Rubenhak.NetflowExporter
{
    internal static class BitConverterEx
    {
        internal static byte[] ToBytes(FieldDefinition field, object value)
        {
            switch (Type.GetTypeCode(value.GetType()))
            {
                case TypeCode.UInt32:
                    {
                        var data = BitConverter.GetBytes((UInt32)value);
                        Array.Reverse(data);
                        return data;
                    }
                case TypeCode.UInt16:
                    {
                        var data = BitConverter.GetBytes((UInt16)value);
                        Array.Reverse(data);
                        return data;
                    }
                case TypeCode.Byte:
                    {
                        var data = new byte[1];
                        data[0] = (byte)value;
                        return data;
                    }

                case TypeCode.String:
                    {
                        //if (((string)value).Length > field.Size)
                        //{
                        //    throw new ArgumentException("Provided string is too long.");
                        //}
                        //byte[] data = new byte[field.Size];
                        //var charData = ((string)value).ToCharArray();
                        //Buffer.BlockCopy(charData, 0, data, 0, Math.Min(charData.Length, data.Length));
                        //return data;

                        var length = (ushort)((string)value).Length;
                        if (length > field.Size)
                        {
                            throw new ArgumentException($"Invalid length of string:{length} expected:{field.Size}");
                        }

                        byte[] data = new byte[field.Size == 65535 ? length + 1 : field.Size];
                        var charData = System.Text.Encoding.ASCII.GetBytes((string)value);

                        if (field.Size == 65535)
                        {
                            /*
                             65535 is used to designate a variable length element, which stores its length as the first
                             byte of the element value.  This is required for fields such as 82, InterfaceName
                             See: https://tools.ietf.org/html/rfc7011#section-7 for more details
                            */

                            // copy the length of the string as the first element
                            var lengthBytes = BitConverter.GetBytes(length);
                            Buffer.BlockCopy(lengthBytes, 0, data, 0, lengthBytes.Length);
                            Buffer.BlockCopy(charData, 0, data, 0, Math.Min(charData.Length, data.Length));
                        }
                        else
                        {
                            // treat elements that do not have a size of 65535 as non-variable
                            Buffer.BlockCopy(charData, 0, data, 0, Math.Min(charData.Length, data.Length) * sizeof(ushort));
                        }

                        return data;
                    }

                case TypeCode.Object:
                    {
                        if (value is IPAddress)
                        {
                            var data = ((IPAddress)value).GetAddressBytes();
                            return data;
                        }
                    }
                    break;
            }

            throw new ArgumentException("Invalid value provided.");
        }

    }

}
