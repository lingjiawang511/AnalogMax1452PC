//using System.Windows.Forms;
using System;
using System.Windows.Forms;
using System.IO.Ports;
using System.Drawing;
using System.IO;
using System.Text;
using System.Collections;
using SerialPortAssistant.Properties;

namespace SerialPortAssistant
{
    class TimerAction
    {
    
       public static void timer1Action(FrmMain fm,int type)
        {
          
        }
       #region Modbus读保持寄存器全局变量
       /// <summary>
       /// Modbus读保持寄存器全局变量
       /// </summary> 
       public static int SendCount = 0;
       // public static byte[] TxBuf = { 0x03, 0x03, 0xAA, 0x55, 0x9B, 0x5A };
       public static byte[] TxBuf = new byte[8];
     //  public static byte[] TxBuftest = {0x03,0x04,0x00,0x00,0x5b,0x79,0x23,0x21};
       public static byte[] Txtest = new byte[1];
       public static int ModbusTimeOut = 0;
       public static int SerialRecOk = 0;
       public static byte ModbusAddr = 0;
       public static byte ModbusFunCode = 0x03;
       public static ushort ModbusStarRegister = 0;
       public static ushort ModbusNumRegister = 2;
       public static bool frameDelayType = false;   //处理帧延时变量，
       public static int ReadLenth =9;
       #endregion
       #region Modbus读保持寄存器主机发送代码03H
       /// <summary>
       /// Modbus读保持寄存器发送主机代码03H
       /// </summary>
       public static void timer2Action_SendFc03(byte address, ushort start, ushort registers, FrmMain fm)
       {
           if (fm.button1.Text == "关闭串口")
           {
               if (SendCount == 0)
               {
                   if (frameDelayType == false)
                        { SendCount = 1; }
                   else
                        { SendCount = 0; }

                   ModbusRTU.BuildMessage(address, 0x03, start, registers, ref TxBuf);
                   ReadLenth = 2 * ModbusNumRegister + 5;//酱紫使用帧延时模式时只需要计算一次
                   fm.serialPort1.ReceivedBytesThreshold = ReadLenth;
               }
               if (ModbusTimeOut == 0)
               {
                   if (frameDelayType ==false)
                   {
                       SendCommand.SendBytes(TxBuf, TxBuf.Length, fm);
                       ModbusTimeOut = 1;
                       fm.timer1.Enabled = true;
                       SendCount = 0;
                   }
                   else
                   {
                     //  byte[] Txtest = new byte[1];
                       Txtest[0] = TxBuf[SendCount++];
                       SendCommand.SendOneByte(Txtest, fm);
                       if (SendCount >= TxBuf.Length)
                       {//定时器1用来做帧延时，定时器2做Modbus超时
                           ModbusTimeOut = 1;
                           fm.timer1.Enabled = true;
                           SendCount = 0;
                       }
                   }
               }            
               if (SerialRecOk == 1)
               {
                   ModbusTimeOut = 0;
                   SerialRecOk = 0;
                   SendCount = 0;
                   fm.timer1.Enabled = false;
               }
           }
          else 
           {
               fm.timer2.Enabled = false;
               fm.timer1.Enabled = false;
           }
       }
       #endregion
       #region Modbus写保持寄存器主机发送代码10H
       /// <summary>
       /// Modbus写保持寄存器发送主机代码10H
       /// </summary>
       public static void timer2Action_SendFc10(byte address, ushort start, ushort registers, ushort[] values, FrmMain fm)
       {
           if (fm.button1.Text == "关闭串口")
           {//此处使用动态内存，当使用帧延时发送时会导致每次都算一遍数据
               byte[] message = new byte[9 + 2 * registers];
               message[6] = (byte)(registers * 2);
               //字节计数，寄存器数量*2
               message[6] = (byte)(registers * 2);
               //保存的数据（需写入的数据）
               for (int i = 0; i < registers; i++)
               {
                   message[7 + 2 * i] = (byte)(values[i] >> 8);
                   message[8 + 2 * i] = (byte)(values[i]);
               }
               ModbusRTU.BuildMessage(address, 0x10, start, registers, ref message);
               ReadLenth = 8;
               fm.serialPort1.ReceivedBytesThreshold = ReadLenth;
               if (SendCount == 0)
               {
                   if (frameDelayType == false)
                   { SendCount = 1; }
                   else
                   { SendCount = 0; }
               }
               if (ModbusTimeOut == 0)
               {
                   if (frameDelayType == false)
                   {
                       SendCommand.SendBytes(message, message.Length, fm);
                       ModbusTimeOut = 1;
                       fm.timer1.Enabled = true;
                       SendCount = 0;
                   }
                   else
                   {
                       //  byte[] Txtest = new byte[1];
                       Txtest[0] = message[SendCount++];
                       SendCommand.SendOneByte(Txtest, fm);
                       if (SendCount >= message.Length)
                       {//定时器1用来做帧延时，定时器2做Modbus超时
                           ModbusTimeOut = 1;
                           fm.timer1.Enabled = true;
                           SendCount = 0;
                       }
                   }
               }
               if (SerialRecOk == 1)
               {
                   ModbusTimeOut = 0;
                   SerialRecOk = 0;
                   SendCount = 0;
                   fm.timer1.Enabled = false;
               }
           }
           else
           {
               fm.timer2.Enabled = false;
               fm.timer1.Enabled = false;
           }      
    
       }
       #endregion
       #region 处理响应程序
       /// <summary>
       /// 处理响应程序
       /// </summary>
       public static void DisposeResponse(byte functionCode, ushort registers, ref ushort[] returnValues, ref byte[] responseData,ushort responseDataLength, ref string CommMessage)
        {
            if (functionCode == 0x03)
            {
               // byte[] response = new byte[5 + 2 * registers];
                if (ModbusRTU.CheckResponse(responseData, responseDataLength))
                {
                    if (responseData[1] > 0x80)//属于不正常响应
                    {
                        ModbusRTU.AbnormalResult((byte)responseData[2], ref CommMessage);
                     //   return false;
                    }
                    else
                    {
                        CommMessage = "正常";
                        for (int i = 0; i < (responseDataLength - 5) / 2; i++)
                        {
                            returnValues[i] = responseData[2 * i + 3];
                            returnValues[i] <<= 8;
                            returnValues[i] += responseData[2 * i + 4];
                        }
                      //  return true;
                    }
                }
                else//CRC错误
                {
                    CommMessage = "CRC错误";
                    /*
                    //CRC错误也读数出来，避免显示0值
                    for (int i = 0; i < (responseData.Length - 5) / 2; i++)
                    {
                        returnValues[i] = responseData[2 * i + 3];
                        returnValues[i] <<= 8;
                        returnValues[i] += responseData[2 * i + 4];
                    }
                     * */
                 //   return false;
                }
            }
            if (functionCode == 0x10)
            {
                if (ModbusRTU.CheckResponse(responseData, responseDataLength))
                {
                    if (responseData[1] > 0x80)//属于不正常响应
                    {
                        ModbusRTU.AbnormalResult((byte)responseData[2], ref CommMessage);
                        //   return false;
                    }
                    else
                    {
                        CommMessage = "正常";
                        for (int i = 0; i < (responseDataLength - 4) / 4; i++)
                        {//返回起始地址和寄存器数量共4个字节,此处先减少为2，因为申请内存的问题
                            returnValues[i] = responseData[2 * i + 2];
                            returnValues[i] <<= 8;
                            returnValues[i] += responseData[2 * i + 3];
                        }
                        //  return true;
                    }
                }
                else//CRC错误
                {
                    CommMessage = "CRC错误";
                    /*
                    //CRC错误也读数出来，避免显示0值
                    for (int i = 0; i < (responseData.Length - 5) / 2; i++)
                    {
                        returnValues[i] = responseData[2 * i + 3];
                        returnValues[i] <<= 8;
                        returnValues[i] += responseData[2 * i + 4];
                    }
                     * */
                    //   return false;
                }  
            }
        }
       #endregion
       #region CRC 字节值表
       //字地址 0 - 255 (只取低8位)
       //位地址 0 - 255 (只取低8位)

       /* CRC 高位字节值表 */
       private static readonly byte[] auchCRCHi = {
            0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0, 0x80, 0x41, 0x01, 0xC0,
            0x80, 0x41, 0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0, 0x80, 0x41,
            0x00, 0xC1, 0x81, 0x40, 0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0,
            0x80, 0x41, 0x01, 0xC0, 0x80, 0x41, 0x00, 0xC1, 0x81, 0x40,
            0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0, 0x80, 0x41, 0x00, 0xC1,
            0x81, 0x40, 0x01, 0xC0, 0x80, 0x41, 0x01, 0xC0, 0x80, 0x41,
            0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0, 0x80, 0x41, 0x00, 0xC1,
            0x81, 0x40, 0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0, 0x80, 0x41,
            0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0, 0x80, 0x41, 0x01, 0xC0,
            0x80, 0x41, 0x00, 0xC1, 0x81, 0x40, 0x00, 0xC1, 0x81, 0x40,
            0x01, 0xC0, 0x80, 0x41, 0x01, 0xC0, 0x80, 0x41, 0x00, 0xC1,
            0x81, 0x40, 0x01, 0xC0, 0x80, 0x41, 0x00, 0xC1, 0x81, 0x40,
            0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0, 0x80, 0x41, 0x01, 0xC0,
            0x80, 0x41, 0x00, 0xC1, 0x81, 0x40, 0x00, 0xC1, 0x81, 0x40,
            0x01, 0xC0, 0x80, 0x41, 0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0,
            0x80, 0x41, 0x01, 0xC0, 0x80, 0x41, 0x00, 0xC1, 0x81, 0x40,
            0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0, 0x80, 0x41, 0x01, 0xC0,
            0x80, 0x41, 0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0, 0x80, 0x41,
            0x00, 0xC1, 0x81, 0x40, 0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0,
            0x80, 0x41, 0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0, 0x80, 0x41,
            0x01, 0xC0, 0x80, 0x41, 0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0,
            0x80, 0x41, 0x00, 0xC1, 0x81, 0x40, 0x00, 0xC1, 0x81, 0x40,
            0x01, 0xC0, 0x80, 0x41, 0x01, 0xC0, 0x80, 0x41, 0x00, 0xC1,
            0x81, 0x40, 0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0, 0x80, 0x41,
            0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0, 0x80, 0x41, 0x01, 0xC0,
            0x80, 0x41, 0x00, 0xC1, 0x81, 0x40
        };
       /* CRC低位字节值表*/
       private static readonly byte[] auchCRCLo = {
            0x00, 0xC0, 0xC1, 0x01, 0xC3, 0x03, 0x02, 0xC2, 0xC6, 0x06,
            0x07, 0xC7, 0x05, 0xC5, 0xC4, 0x04, 0xCC, 0x0C, 0x0D, 0xCD,
            0x0F, 0xCF, 0xCE, 0x0E, 0x0A, 0xCA, 0xCB, 0x0B, 0xC9, 0x09,
            0x08, 0xC8, 0xD8, 0x18, 0x19, 0xD9, 0x1B, 0xDB, 0xDA, 0x1A,
            0x1E, 0xDE, 0xDF, 0x1F, 0xDD, 0x1D, 0x1C, 0xDC, 0x14, 0xD4,
            0xD5, 0x15, 0xD7, 0x17, 0x16, 0xD6, 0xD2, 0x12, 0x13, 0xD3,
            0x11, 0xD1, 0xD0, 0x10, 0xF0, 0x30, 0x31, 0xF1, 0x33, 0xF3,
            0xF2, 0x32, 0x36, 0xF6, 0xF7, 0x37, 0xF5, 0x35, 0x34, 0xF4,
            0x3C, 0xFC, 0xFD, 0x3D, 0xFF, 0x3F, 0x3E, 0xFE, 0xFA, 0x3A,
            0x3B, 0xFB, 0x39, 0xF9, 0xF8, 0x38, 0x28, 0xE8, 0xE9, 0x29,
            0xEB, 0x2B, 0x2A, 0xEA, 0xEE, 0x2E, 0x2F, 0xEF, 0x2D, 0xED,
            0xEC, 0x2C, 0xE4, 0x24, 0x25, 0xE5, 0x27, 0xE7, 0xE6, 0x26,
            0x22, 0xE2, 0xE3, 0x23, 0xE1, 0x21, 0x20, 0xE0, 0xA0, 0x60,
            0x61, 0xA1, 0x63, 0xA3, 0xA2, 0x62, 0x66, 0xA6, 0xA7, 0x67,
            0xA5, 0x65, 0x64, 0xA4, 0x6C, 0xAC, 0xAD, 0x6D, 0xAF, 0x6F,
            0x6E, 0xAE, 0xAA, 0x6A, 0x6B, 0xAB, 0x69, 0xA9, 0xA8, 0x68,
            0x78, 0xB8, 0xB9, 0x79, 0xBB, 0x7B, 0x7A, 0xBA, 0xBE, 0x7E,
            0x7F, 0xBF, 0x7D, 0xBD, 0xBC, 0x7C, 0xB4, 0x74, 0x75, 0xB5,
            0x77, 0xB7, 0xB6, 0x76, 0x72, 0xB2, 0xB3, 0x73, 0xB1, 0x71,
            0x70, 0xB0, 0x50, 0x90, 0x91, 0x51, 0x93, 0x53, 0x52, 0x92,
            0x96, 0x56, 0x57, 0x97, 0x55, 0x95, 0x94, 0x54, 0x9C, 0x5C,
            0x5D, 0x9D, 0x5F, 0x9F, 0x9E, 0x5E, 0x5A, 0x9A, 0x9B, 0x5B,
            0x99, 0x59, 0x58, 0x98, 0x88, 0x48, 0x49, 0x89, 0x4B, 0x8B,
            0x8A, 0x4A, 0x4E, 0x8E, 0x8F, 0x4F, 0x8D, 0x4D, 0x4C, 0x8C,
            0x44, 0x84, 0x85, 0x45, 0x87, 0x47, 0x46, 0x86, 0x82, 0x42,
            0x43, 0x83, 0x41, 0x81, 0x80, 0x40
        };
       #endregion
       #region 查表法生成CRC码
       /// <summary>
       /// 查表法生成CRC码
       /// </summary>
       public static void crc16(byte[] puchMsg, UInt32 usDataLen, ref byte[] CRC)
        {

            byte uchCRCHi = 0xFF; /* 高CRC字节初始化 */
            byte uchCRCLo = 0xFF; /* 低CRC 字节初始化 */
          //  byte uIndex=0; /* CRC循环中的索引 */
            ushort uIndex = 0x0000;
            ushort i=0;
            while (usDataLen-->0)
            { /* 传输消息缓冲区 */
                uIndex = Convert.ToByte( uchCRCHi ^ puchMsg[i++]); /* 计算CRC */
                uchCRCHi =Convert.ToByte( uchCRCLo ^ auchCRCHi[uIndex]);
                uchCRCLo = auchCRCLo[uIndex];
            }
           // return (Convert.ToUInt16((Convert.ToUInt16(uchCRCHi) << 8 )| uchCRCLo));
            CRC[0] = uchCRCHi;
            CRC[1] = uchCRCLo;
        }
       #endregion
    }
}
