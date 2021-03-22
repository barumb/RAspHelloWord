using System;
using System.Collections.Generic;
using System.Device.Gpio;
using System.Threading;
using System.Threading.Tasks;
using RaspHelloWord;

namespace RaspHelloWord
{

   public abstract class DPdev485
   {
        //private int speed;
        //private string parity;
        //private int nrBitStop;

        public abstract byte[] QueryDevice (byte[] pSendFrame);


        private Single BytesToSingle(byte[] Buffer, int StartIndex)
        {
            byte[] _internal = new byte[4];
            _internal[3]= Buffer[StartIndex +0];
            _internal[2]= Buffer[StartIndex +1];
            _internal[1]= Buffer[StartIndex +2];
            _internal[0]= Buffer[StartIndex +3];

            return BitConverter.ToSingle(_internal);
        }

        public virtual  byte[] SwitchMsbLsb(ushort Val16bit)
        {
            byte[] _tmp = new byte[2];
            byte[] _modbus = new byte[2];

            _tmp = BitConverter.GetBytes(Val16bit);
            _modbus[1] = _tmp[0];
            _modbus[0] = _tmp[1];

            return _modbus;
        }


        /// <summary>
        /// Calcola il CRC di un frame MODBUS: addr+msg+CRC.
        /// </summary>
        /// <param name="msgFrame"></param>
        /// <returns></returns>
        public virtual  byte[] Modbus_Crc16(byte[] msgFrame)
         {
            byte crcRegister_H = 0xFF, crcRegister_L = 0xFF; // presets a 16-bit register value 0xFFFF
            byte polynomialCode_H = 0xA0, polynomialCode_L = 0x01; // polynomial code 0xA001

            for (int i = 0; i < msgFrame.Length; i++)
            {
                crcRegister_L = (byte)(crcRegister_L ^ msgFrame[i]);

                for (int j = 0; j < 8; j++)
                {
                    byte tempCRC_H = crcRegister_H;
                    byte tempCRC_L = crcRegister_L;

                    crcRegister_H = (byte)(crcRegister_H >> 1);
                    crcRegister_L = (byte)(crcRegister_L >> 1);
                    // Finally, a bit after the first should be the lower right front upper right: If the last digit is a high-low 1 right up front
                    if ((tempCRC_H & 0x01) == 0x01)
                    {
                        crcRegister_L = (byte)(crcRegister_L | 0x80);
                    }

                    if ((tempCRC_L & 0x01) == 0x01)
                    {
                        crcRegister_H = (byte)(crcRegister_H ^ polynomialCode_H);
                        crcRegister_L = (byte)(crcRegister_L ^ polynomialCode_L);
                    }
                }
            }

            return new byte[] { crcRegister_L, crcRegister_H };
         }
        
        // CRC16-X25 utilizzato dall'inverter Autora PowerOne
        // Il metodo restituisce il CRC già con i MSB e LSB invertiti in modo da essere 
        // pronto ad essere copiato nel frame da trasmettere
        public virtual  byte[] CCITT_CRC16(byte[] msgFrame)
        {
                ushort data;
                ushort crc = 0xFFFF;
                

                for (int j = 0; j < msgFrame.Length; j++)
                {
                    crc = (ushort)(crc ^ msgFrame[j]);
                    for (int i = 0; i < 8; i++)
                    {
                        if ((crc & 0x0001) == 1)
                            crc = (ushort)((crc >> 1) ^ 0x8408);
                        else
                            crc >>= 1;
                    }
                }
                crc = (ushort)~crc;
                data = crc;
                crc = (ushort)((crc << 8) ^ (data >> 8 & 0xFF));
                byte[] crcX25 = BitConverter.GetBytes(crc);
                //inverto MSB e LSB
                byte tmp = crcX25[0];
                crcX25[0]=crcX25[1];
                crcX25[1]= tmp;
                return crcX25;
                
        }
        //public abstract void SetSerial();
   } //class

} // di namespace