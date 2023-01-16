using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServer
{
	public enum PROTOCOL : short
	{
		BEGIN = 0,

        CHAT_MSG_REQ = 1,
        CHAT_MSG_ACK = 2,
        ACCOUNT_LOGIN_REQ = 3,
        ACCOUNT_LOGIN_ACK = 4,
        ACCOUNT_CREATE_REQ = 5,
        ACCOUNT_CREATE_ACK = 6,
        CHARACTER_DATA_SAVE_REQ = 7,
        CHARACTER_DATA_SAVE_ACK = 8,
        CHARACTER_DATA_GET_REQ = 9,
        CHARACTER_DATA_GET_ACK = 10,

        END
    }
}
