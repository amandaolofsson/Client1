using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;

namespace Client1
{
    class Session
    {
        NetworkStream tcpStream;
        byte[] bRead = new byte[8192];
        private int key = 4;
        Queue<string> responses = new Queue<string>();

        public Session(NetworkStream tcpStream)
        {
            this.tcpStream = tcpStream;
        }

        public void Run()
        {
            while (true)
            {
                Tuple<ServerMessageEnum, string> serverReply = Receive();
                if (serverReply.Item2 == "$$DISCONNECT")
                {
                    throw new DisconnectExceptions();
                }

                if (serverReply.Item1 == ServerMessageEnum.Response)
                {
                    Console.WriteLine(serverReply.Item2);
                    Send(Console.ReadLine());
                }
                else if (serverReply.Item1 == ServerMessageEnum.ResponseEncrypted)
                {
                    Console.WriteLine(serverReply.Item2);
                    string s = Console.ReadLine();
                    s = Encrypt(s, key);
                    Send(s);
                }
                else if (serverReply.Item1 == ServerMessageEnum.Message)
                {
                    string[] messageParts = serverReply.Item2.Split('|');
                    Console.WriteLine("{0} {1}: {2}", messageParts[0], messageParts[1], Decrypt(messageParts[2], key));
                }
                else
                {
                    Console.WriteLine(serverReply.Item2);
                }
            }
        }

        //Reads respons from server, returns a tuple type with message type and content
        public Tuple<ServerMessageEnum, String> Receive()
        {
            string s;

            if (responses.Count > 0)
            {
                s = responses.Dequeue();
            }
            else
            {
                int bReadSize = tcpStream.Read(bRead, 0, bRead.Length);
                s = System.Text.Encoding.UTF8.GetString(bRead, 0, bReadSize);
            }

            //Splits server respone into messages by splitting on the EOM-character (¤)
            string[] parts = s.Split('¤');
            
            if(parts.Length > 1)
            {
                s = parts[0];
                for(int i = 1; i<parts.Length; i++)
                {
                    if(!string.IsNullOrEmpty(parts[i]))
                    {
                        responses.Enqueue(parts[i]);
                    }
                }
            }
            
            string command = s.Substring(0, 1);
            ServerMessageEnum type;
            if (command == "R")
            {
                type = ServerMessageEnum.Response;
            }
            else if (command == "E")
            {
                type = ServerMessageEnum.ResponseEncrypted;
            }
            else if (command == "M")
            {
                type = ServerMessageEnum.Message;
            }
            else
            {
                type = ServerMessageEnum.Text;
            }
            return new Tuple<ServerMessageEnum, string>(type, s.Substring(1));
        }

        public string Encrypt(string text, int key)
        {
            string encryptText = "";

            foreach (char ch in text)
            {
                encryptText += Cipher(ch, key);
            }
            return encryptText;
        }

        public void Send(string message)
        {
            Byte[] bSend = System.Text.Encoding.UTF8.GetBytes(message);
            tcpStream.Write(bSend, 0, bSend.Length);
        }

        public string Decrypt(string text, int key)
        {
            return Encrypt(text, 26 - key);
        }

        public char Cipher(char ch, int key)
        {
            if (!Char.IsLetter(ch))
            {
                return ch;
            }
            char d = char.IsUpper(ch) ? 'A' : 'a';
            return (char)((((ch + key) - d) % 26) + d);
        }

    }
}
