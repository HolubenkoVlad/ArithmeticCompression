using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp1
{
    class ArithmeticCompression
    {
        string message;
        string writePath = @"fileLab1Result.bin";
        int sizeBits;
        List<char> symbols = new List<char>();
        List<int> enters = new List<int>();
     //   List<string> bits = new List<string>();


        public ArithmeticCompression()
        {
            message = ReadFile();
        }

        string ReadFile()
        {
            string filename = "fileLab1.txt";
            using (StreamReader sr = new StreamReader(filename, System.Text.Encoding.Default))
            {
                return sr.ReadToEnd();
            }
        }

        int findInList(char symbol)
        {
            for (int i = 0; i < symbols.Count; i++)
            {
                if (symbols[i] == symbol)
                {
                    return i;
                }
            }
            return -1;
        }

        public void WriteCodeTable()
        {
            int tempNumber;
            for (int i = 0; i < message.Length; i++)
            {
                tempNumber = findInList(message[i]);
                if (symbols.Count == 0 || tempNumber == -1)
                {
                    symbols.Add(message[i]);
                    enters.Add(1);
                }
                else
                {
                    enters[tempNumber] += 1;
                }
            }
            SortAllArrays();
            sizeBits = CalculateSizeBits();
            //for (int i = 0; i < symbols.Count; i++)
            //{
            //    bits.Add(ByteToBit(i));
            //}
        }

        void SortAllArrays()
        {
            int temp;
            char temp2;
            for (int i = 0; i < enters.Count - 1; i++)
            {
                for (int j = i + 1; j < enters.Count; j++)
                {
                    if (enters[i] < enters[j])
                    {
                        temp = enters[i];
                        temp2 = symbols[i];
                        enters[i] = enters[j];
                        symbols[i] = symbols[j];
                        enters[j] = temp;
                        symbols[j] = temp2;
                    }
                }
            }
        }

        int CalculateSizeBits()
        {
            return Convert.ToString(symbols.Count - 1, 2).Length;
        }

        //string ByteToBit(int num)
        //{
        //    string temp = "";
        //    for (byte i = 0; i < sizeBits; i++/*, num /= 2*/)
        //    {
        //        if ((num >> i & 1) == 1)
        //        //if ((num % 2) == 1)
        //        {
        //            temp = temp.Insert(0, "1");
        //        }
        //        else temp = temp.Insert(0, "0");

        //    }
        //    return temp;

        //}

        public void pack()
        {
            WriteCodeTable();
            byte[] arrayBytes = MessageToBits();
            using (BinaryWriter sw = new BinaryWriter(File.Open(writePath, FileMode.Create)))
            {
                sw.Write(sizeBits);
                sw.Write(new string(symbols.ToArray()));
                foreach (byte temp in arrayBytes)
                    sw.Write(temp);
            }
        }

        public byte[] MessageToBits()
        {
            List<byte> listBytes = new List<byte>() {0};
            int counterBytes = 0;
            int capacityByte = 0;
            for(int i = 0; i < message.Length; i++)
            {
                if (capacityByte+sizeBits > 8)
                {
                    listBytes[counterBytes] = (byte)(listBytes[counterBytes] | (Array.IndexOf(symbols.ToArray(), message[i]) << capacityByte));
                    listBytes.Add(0);
                    counterBytes++;
                    listBytes[counterBytes] = (byte)(listBytes[counterBytes] | (Array.IndexOf(symbols.ToArray(), message[i]) >> (8-capacityByte)));
                    capacityByte = sizeBits - (8 - capacityByte);
                }
                else
                {
                    listBytes[counterBytes] = (byte)(listBytes[counterBytes] | (Array.IndexOf(symbols.ToArray(), message[i]) << capacityByte));
                    capacityByte += sizeBits;
                }

            }
            return listBytes.ToArray();
        }

        public void unpack()
        {
            int sizeB;
            string codeT;
            List<byte> list = new List<byte>();
            using (BinaryReader br = new BinaryReader(File.Open(writePath, FileMode.Open)))
            {
                sizeB = br.ReadInt32();
                codeT = br.ReadString();
                while (br.BaseStream.Position != br.BaseStream.Length)
                {
                    list.Add(br.ReadByte());
                }
            }
            Console.WriteLine(DecompressionMessage(codeT,sizeB, ref list));
        }

        byte ConcatMask(int countBits)
        {
            byte num = 0;
            for (int i = 0; i < countBits; i++)
                    num += (byte)Math.Pow(2, i);
            return num;
        }

        string DecompressionMessage(string codeTable, int lengthMessage, ref List<byte> messageFromFile)
        {
            string resultString = "";
            byte mask = 255, concatMask,concat = 0;
            byte sizeConcatMask;
            int counterByte = 0;
            for(int i=0;i<messageFromFile.Count-1;i++)
            {

                while (counterByte + lengthMessage <= 8)
                {
                    resultString += codeTable[messageFromFile[i] & (mask >> (8-lengthMessage))];
                    counterByte += lengthMessage;
                    messageFromFile[i] = (byte)(messageFromFile[i] >> lengthMessage);                  
                }              
                if (counterByte!= 8)
                {
                    concat = (byte)(messageFromFile[i] & (mask >> (8 - lengthMessage)));
                    sizeConcatMask = (byte)(lengthMessage - (8 - counterByte));
                    concatMask = (byte)((messageFromFile[i + 1] & ConcatMask(sizeConcatMask))<<(lengthMessage-sizeConcatMask));
                    resultString += codeTable[concat | concatMask];
                    messageFromFile[i+1] = (byte)(messageFromFile[i+1] >> sizeConcatMask);
                    counterByte = sizeConcatMask;
                }
                else
                {
                    counterByte = 0;
                }
            }
            //string receviedMessage = "";
            //for (int i = 0; i < messageFromFile.Length; i += lengthMessage)
            //{
            //    receviedMessage += codeTable.Cast<string[]>().Where(c => c[1] == messageFromFile.Substring(i, lengthMessage)).First()[0];
            //}
            //return receviedMessage;
            return resultString;
        }

    }

}
