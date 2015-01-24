using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Bomberman.Network
{
    public sealed class LoginPayload
    {
        public const int MaxTokenLength = 5;
        public const int MaxNameLength = 10;

        public string Token { get; set; }
        public string Name { get; set; }
        public int Version { get; set; }

        /// <summary>
        /// True if this login payload is valid.
        /// </summary>
        public bool Valid
        {
            get
            {
                return Token != null && Token.Length > 0 && Token.Length <= MaxTokenLength && Name != null && Name.Length > 0 && Name.Length <= MaxNameLength;
            }
        }

        /// <summary>
        /// True if this login payload is compatible.
        /// </summary>
        public bool Compatible
        {
            get
            {
                return Version == Protocol.Version;
            }
        }

        public void Write(BinaryWriter w)
        {
            w.Write(Token);
            w.Write(Name);
            w.Write(Version);
        }

        public static LoginPayload Read(BinaryReader r)
        {
            return new LoginPayload()
            {
                Token = r.ReadString(),
                Name = r.ReadString(),
                Version = r.ReadInt32(),
            };
        }

        public LoginPayload(string token, string name, int version)
        {
            Token = token;
            Name = name;
            Version = version;
        }

        public LoginPayload() : this("", "", 0) { }
    }
}