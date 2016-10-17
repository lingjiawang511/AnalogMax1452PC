using System;
using System.Windows.Forms;
using System.Text;

namespace SerialPortAssistant
{
    class SendCommand
    {
        /// <summary>
        /// 执行指定的命令
        /// </summary>
        /// <param name="command">指定的命令</param>
        /// <param name="f1">发送命令的窗体</param>
        public static void action(string command, FrmMain fm)
        {
            try
            {
                string open = command;
                char[] send=new char[open.Length];

                for (int i = 0; i < send.Length; i++)
                {
                     send[i] =open[i];
                }

                fm.serialPort1.Write(send, 0, send.Length);
            }
            catch (Exception err)
            {
                fm.timer1.Enabled = false;
               // fm.groupBox2.Enabled = true;
                fm.button1.Enabled = false;
                MessageBox.Show(err.Message);
            }
        }
        public static void SendBytes(byte[] command,int length, FrmMain fm)
        {
            try
            {
                byte[] open = command;
                byte[] send = new byte[open.Length];
                for (int i = 0; i < send.Length; i++)
                {
                    send[i] = open[i];
                    
                }
                if (length >= send.Length)
                    length = send.Length;
                fm.serialPort1.Write(send, 0, length);
              //  fm.serialPort1.Write(" ");
            }
            catch (Exception err)
            {
                fm.timer1.Enabled = false;
             //   fm.groupBox2.Enabled = true;
                fm.button1.Enabled = false;
                MessageBox.Show(err.Message);
            }
        }
        public static void SendOneByte(byte[] TByte, FrmMain fm)
        {
            SendCommand.SendBytes(TByte, 1, fm);
        }

        #region 将一组字符串转化为相应的16进制数，后缀加上h
        /// <summary>
        /// 将一组字符串转化为相应的16进制数，后缀加上h
        /// </summary>
        /// <param name="Str">需要转换的字符串</param>
        /// <param name="resultvalue">字符串转换后数据存放的数组</param>
        /// <param name="Offset">数据存放在数据的偏移量</param>
        public static void ConvertStringToHex(string Str, ref byte[] resultvalue, ref int Offset)
        {
            StringBuilder sb = new StringBuilder();
            int j = Offset;
            var str = Str;
            foreach (char c in str)
            {
                if ((c >= '0' && c <= '9') || (c >= 'A' && c <= 'F') || (c >= 'a' && c <= 'f'))
                {
                    sb.Append(c);
                }
                else if (c == 'h' || c == 'H')
                {
                    int i;
                    if (int.TryParse(sb.ToString(), System.Globalization.NumberStyles.HexNumber, null, out i))
                        resultvalue[j++] = (byte)i;
                    sb.Remove(0, sb.Length);
                    continue;
                }
                else if (c == ' ')
                {
                    sb.Remove(0, sb.Length);
                    continue;
                }
                else
                    break;
            }
            Offset = j;
        }
        #endregion 
        #region
        /// <summary>
        /// 将一组字符串转化为相应的10进制数
        /// </summary>
        /// <param name="Str">需要转换的字符串</param>
        /// <param name="resultvalue">字符串转换后数据存放的数组</param>
        /// <param name="Offset">数据存放在数据的偏移量</param>
        public static void ConvertStringToInte(string Str, ref byte[] resultvalue, ref int Offset)
        {
            StringBuilder sb = new StringBuilder();
            int j = Offset;
            var str = Str;
            foreach (char c in str)
            {
                if (c >= '0' && c <= '9')
                {
                    sb.Append(c);
                }
                else if (c == ' ')
                {
                    int i;
                    if (int.TryParse(sb.ToString(), System.Globalization.NumberStyles.Integer, null, out i))
                        resultvalue[j++] = (byte)i;
                    sb.Remove(0, sb.Length);
                    continue;
                }
                else
                    break;
            }
            Offset = j;
        }
        #endregion
    }
}
