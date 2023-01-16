using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FreeNet;

namespace CSampleServer
{
	using GameServer;
	using System.Runtime.InteropServices.ComTypes;
	using static CSampleServer.Program;
	using static System.Net.Mime.MediaTypeNames;

	/// <summary>
	/// 하나의 session객체를 나타낸다.
	/// </summary>
	class CGameUser : IPeer
	{
		public delegate List<CGameUser> GetTokenListCallBack();
		public GetTokenListCallBack callback_get_tokenlist;
		CUserToken token;

		public CGameUser(CUserToken token)
		{
			this.token = token;
			this.token.set_peer(this);
		}

		void IPeer.on_message(Const<byte[]> buffer)
		{
			// ex)
			CPacket msg = new CPacket(buffer.Value, this);
			PROTOCOL protocol = (PROTOCOL)msg.pop_protocol_id();
			Console.WriteLine("------------------------------------------------------");
			Console.WriteLine("protocol id " + protocol);
			switch (protocol)
			{
				case PROTOCOL.CHAT_MSG_REQ:
					{
						string text = msg.pop_string();
						Console.WriteLine(string.Format("text {0}", text));
						//Program.chatlog_list.Add(text);
						CPacket response = CPacket.create((short)PROTOCOL.CHAT_MSG_ACK);
						response.push(text);
						//send(response);
						sendAll(response);
					}
					break;
				case PROTOCOL.ACCOUNT_LOGIN_REQ:
					{
						string[] accountInfo = msg.pop_string().Split('\n'); // id pw
						string id = accountInfo[0];
						string pw = accountInfo[1];

                        CPacket response = CPacket.create((short)PROTOCOL.ACCOUNT_LOGIN_ACK);

                        if (AccountCheckLogin(id, pw))
						{
                            response.push("Success");
                            send(response);
                        }
                        else
						{
                            response.push("Fail");
                            send(response);
                        }
                    }
					break;
				case PROTOCOL.ACCOUNT_CREATE_REQ:
					{
                        string[] accountInfo = msg.pop_string().Split('\n'); // id pw
                        string id = accountInfo[0];
                        string pw = accountInfo[1];

                        CPacket response = CPacket.create((short)PROTOCOL.ACCOUNT_CREATE_ACK);

                        if (CreateAccount(id, pw) == true)
						{
                            response.push("Success");
                            send(response);
                        }
                        else
						{
                            response.push("Fail");
                            send(response);
                        }
                    }
                    break;
            }
        }

		void IPeer.on_removed()
		{
			Console.WriteLine("The client disconnected.");
			remove_user(this);
		}

		public void send(CPacket msg)
		{
			this.token.send(msg);
		}
		
		public void sendAll(CPacket msg)
        {
            List<CGameUser> users = callback_get_tokenlist();

			foreach (CGameUser user in users)
			{
				user.token.send(msg);
			}
		}

		void IPeer.disconnect()
		{
			this.token.socket.Disconnect(false);
		}

		void IPeer.process_user_operation(CPacket msg)
		{
		}
    }
}
