using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using FreeNet;
using GameServer;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;

namespace CSampleServer
{
    class Program
    {
        static MySqlConnection conn;
        static List<CGameUser> userlist;

        public struct CharacterInfo
        {
            public int hp;
            public int mp;
            public int atk;
            public int def;
            public string tribe;
            public string state;
            public string job;
            public bool emotion;
        }

        static void Main(string[] args)
        {
            CPacketBufferManager.initialize(2000);
            userlist = new List<CGameUser>();

            CNetworkService service = new CNetworkService();
            // 콜백 매소드 설정.
            service.session_created_callback += on_session_created;
            // 초기화.
            service.initialize();

            var host = Dns.GetHostEntry(Dns.GetHostName());
            string local_IP = "";

            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                {
                    local_IP = ip.ToString();
                    break;
                }
            }

            //Access mysql database. And check if the connection is successful.
            string connStr = "server=localhost;user=root;database=chatacter_database;port=3306;password=vhzptapahflA123";
            conn = new MySqlConnection(connStr);

            try
            {
                Console.WriteLine("Connecting to MySQL...");
                conn.Open();

                //Check for connection
                if (conn.State == System.Data.ConnectionState.Open)
                {
                    Console.WriteLine("Connection is successful.");
                }
                else
                {
                    Console.WriteLine("Connection is failed.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }

            //service.listen("127.0.0.1", 7979, 100); // IP를 직접 입력하는 방식
            Console.WriteLine(string.Format("Get Local IP -> {0}", local_IP)); // 현재 컴퓨터의 IP 주소를 가져오는 방식
            service.listen(local_IP, 7979, 100); // 포트는 7979로 고정

            Console.WriteLine("Started!");
            while (true)
            {
                //Console.Write(".");
                System.Threading.Thread.Sleep(1000);
            }

            Console.ReadKey();
        }

        /// <summary>
        /// 클라이언트가 접속 완료 하였을 때 호출됩니다.
        /// n개의 워커 스레드에서 호출될 수 있으므로 공유 자원 접근시 동기화 처리를 해줘야 합니다.
        /// </summary>
        /// <returns></returns>
        static void on_session_created(CUserToken token)
        {
            CGameUser user = new CGameUser(token);
            user.callback_get_tokenlist += GetTokenList;

            lock (userlist)
            {
                userlist.Add(user);
            }
        }

        /// <summary>
        /// 클라이언트가 접속 해제를 하였을 때 호출됩니다.
        /// </summary>
        /// <param name="user"></param>
        public static void remove_user(CGameUser user)
        {
            lock (userlist)
            {
                userlist.Remove(user);
            }
        }

        /// <summary>
        /// 서버에 접속한 클라이언트 토큰 리스트를 반환한다.
        /// </summary>
        /// <returns>클라이언트 토큰 리스트</returns>
        public static List<CGameUser> GetTokenList()
        {
            return userlist;
        }

        public static bool AccountCheckLogin(string id, string pw)
        {
            //Check if the parameter id, pw values match the id, pw values in the account_data_table table in the chat_database database. Returns true if there is a match, false if not.
            string sql = "SELECT * FROM account_data_table WHERE id = @id AND pw = @pw";
            MySqlCommand cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@id", id);
            cmd.Parameters.AddWithValue("@pw", pw);
            MySqlDataReader rdr = cmd.ExecuteReader();

            if (rdr.Read())
            {
                rdr.Close();
                return true;
            }

            rdr.Close();
            return false;
        }

        public static bool ChreateAccountCheck(string id)
        {
            //Check if the parameter id value matches the id value in the account_data_table table in the chat_database database. Returns true if there is a match, false if not.
            string sql = "SELECT * FROM account_data_table WHERE id = @id";
            MySqlCommand cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@id", id);
            MySqlDataReader rdr = cmd.ExecuteReader();

            if (rdr.Read())
            {
                rdr.Close();
                return true;
            }

            rdr.Close();
            return false;
        }

        public static bool CreateAccount(string id, string pw)
        {
            //If the id, pw values in the parameter do not match the id, pw values in the account_data_table table in the chat_database database, add the id, pw values to the account_data_table table.
            if (!ChreateAccountCheck(id))
            {
                string sql = "INSERT INTO account_data_table(id,pw, create_date) VALUES(@id,@pw,@date)";
                MySqlCommand cmd = new MySqlCommand(sql, conn);

                cmd.Parameters.AddWithValue("@id", id);
                cmd.Parameters.AddWithValue("@pw", pw);
                cmd.Parameters.AddWithValue("@date", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                cmd.ExecuteNonQuery();

                return true;
            }
            else
            {
                return false;
            }
        }

        public static CharacterInfo SetInitCharacterInfo()
        {
            //Set the initial character information of the parameter id value.
            CharacterInfo characterInfo = new CharacterInfo();

            characterInfo.hp = 100;
            characterInfo.mp = 50;
            characterInfo.atk = 10;
            characterInfo.def = 10;
            characterInfo.tribe = "인간";
            characterInfo.state = "정상";
            characterInfo.job = "모험가";
            characterInfo.emotion = false;

            return characterInfo;
        }

        public static string GetCharacterInfo(string targetID)
        {
            //If the data in the character_data_table table in the character_database database has data such as the targetID parameter in the character_id column data, the data in the character_data column stored like the data in the corresponding character_id column are imported.
            string sql = "SELECT * FROM character_data_table WHERE character_id = @id";
            MySqlCommand cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@id", targetID);
            MySqlDataReader rdr = cmd.ExecuteReader();

            if (rdr.Read())
            {
                //get character_data culum value
                string characterData = rdr.GetString("character_data");

                rdr.Close();
                return characterData;
            }
            else
            {
                rdr.Close();
                CharacterInfo characterInfo = new CharacterInfo();
                characterInfo = SetInitCharacterInfo();

                //Save the targetID parameter and the characterInfo variable in the character_data_table table.
                string sql2 = "INSERT INTO character_data_table(character_id, character_data) VALUES(@id,@data)";
                string data = JsonConvert.SerializeObject(characterInfo);
                
                MySqlCommand cmd2 = new MySqlCommand(sql2, conn);
                cmd2.Parameters.AddWithValue("@id", targetID);
                cmd2.Parameters.AddWithValue("@data", data);
                cmd2.ExecuteNonQuery();
                
                return data;
            }
        }
    }
}
