using Discord.WebSocket;
using System;
using System.Collections.Generic;

namespace Rayiah.Objects.Structs
{
    [Serializable]
    struct UserRolesData
    {
        public ulong user;
        public ulong[] roles;

        /// <summary>
        /// Empty roles, just like the new user.
        /// </summary>
        /// <param name="u"></param>
        public UserRolesData(ulong u)
        {
            user = u;
            this.roles = new ulong[0];
        }

        public UserRolesData(ulong u, ulong[] roles) {
            user = u;
            this.roles = roles;
        }

        public UserRolesData(ulong u, IEnumerator<SocketRole> roles) {
            user = u;
            this.roles = new ulong[0];
            SetRoles(roles);
        }

        public void SetRoles(ulong[] roles)
        {
            this.roles = roles;
        }

        public void SetRoles(IEnumerator<SocketRole> en)
        {
            List<ulong> roles = new List<ulong>();
            while (en.MoveNext()) {
                if (en.Current.IsEveryone || en.Current.IsManaged || en.Current.Id == 0) {
                    continue;
                }
                roles.Add(en.Current.Id);
            }
            this.roles = roles.ToArray();
        }
    }
}