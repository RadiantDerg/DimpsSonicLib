﻿using System;
using DimpsSonicLib.IO;
using System.IO;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Timers;

namespace DimpsSonicLib.Archives
{
    /* TODO:
    *   Clean all this up and use various classes within DimpsSonicLib as well as
    *   using FileInterface.cs for the OBBType detection. Could also stand to use 
    *   class inheritance to clean up all of the extra usings.
    *  
    *   I should instead focus DimpsSonicLib to return arrays and data from the files
    *   instead of handling everything in the library itself. Methods for handling
    *   data should be up to the individual programs that reference this library.
    */

    public class AndroidOBB
    {
        public enum OBBType
        {
            Unknown, DTRZ, LPKv1, LPKv2, NotOBB
        }

        /// <summary>
        /// Attempts to automatically detect and return the APK Expansion type.
        /// </summary>
        /// <param name="input">Takes in file path argument</param>
        /// <returns>The OBB Type</returns>
        public static OBBType DetermineOBBType(string input)
        {
            Stream stream = File.OpenRead(input);
            ExtendedBinaryReader reader = new ExtendedBinaryReader(stream);
            bool isOBB = (Path.GetExtension(input).ToLower() == ".obb");
            var sig = reader.ReadSignature(3);
            reader.JumpTo(0);
            var sig2 = reader.ReadSignature(4);

            if (isOBB)
            {
                if (sig == "LPK")
                {
                    reader.JumpTo(12);
                    var v1Check = reader.ReadUInt32();

                    if (v1Check != 0)
                        return OBBType.LPKv1;
                    else
                        return OBBType.LPKv2;
                }
                else if (sig2 == "DTRZ")
                    return OBBType.DTRZ;
                else
                    return OBBType.Unknown;
            }
            else
                return OBBType.NotOBB;
        }

        /// <summary>
        /// Extracts an OBB of a given typed passed to it.
        /// </summary>
        /// <param name="input">Takes in file path argument</param>
        /// <param name="Type">Takes in OBBType to determine correct extraction method</param>
        public static void ExtractOBBFile(string input, OBBType Type)
        {
            var dirName = Path.ChangeExtension(input, null);
            Directory.CreateDirectory(dirName);

            switch (Type)
            {
                case OBBType.DTRZ:
                    ExtractDTRZFile(input, dirName);
                    break;

                case OBBType.LPKv1:
                    ExtractLPKv1File(input, dirName);
                    break;

                case OBBType.LPKv2:
                    ExtractLPKv2File(input, dirName);
                    break;

                case OBBType.Unknown:
                    throw new NotImplementedException("The given OBB file is not currently supported.");
            }
        }

        #region Extraction
        /// <summary>
        /// Extracts a Sonic 4: Episode I OBB. Not implemented due to proprietary compression format.
        /// </summary>
        /// <param name="input">Takes in file path argument</param>
        /// <param name="baseDir">Takes in a base directory path for writing files</param>
        private static void ExtractDTRZFile(string input, string baseDir)
        {
            throw new NotImplementedException(" DZIP compressed OBBs are not currently supported. Please use the official"
                + " dzip.exe tool\n available in the Marmalade SDK: https://www.madewithmarmalade.com/developer");
        }


        /// <summary>
        /// Extracts Sega Forever versions of Sonic 4: Episode II OBB files.
        /// </summary>
        /// <param name="input">Takes in file path argument</param>
        /// <param name="baseDir">Takes in a base directory path for writing files</param>
        public static void ExtractLPKv2File(string input, string baseDir)
        {
            Stream stream = File.OpenRead(input);
            ExtendedBinaryReader reader = new ExtendedBinaryReader(stream);
            long lastIndexPos;

            reader.JumpTo(6);
            ushort fileCount = reader.ReadUInt16();
            ushort dirCount = reader.ReadUInt16();
            reader.JumpAhead(22);
            uint listPointer = reader.ReadUInt32();
            uint nameTablePointer = reader.ReadUInt32();
            uint nameTableLength = reader.ReadUInt32();
            reader.JumpTo(listPointer);


            for (int i = 0; i < fileCount; i++)
            {
                // Read the index entry.
                uint filePtr = reader.ReadUInt32();
                uint fileSize = reader.ReadUInt32();
                reader.JumpAhead(4);
                ushort dirID = reader.ReadUInt16();
                ushort nameID = reader.ReadUInt16();
                lastIndexPos = reader.BaseStream.Position;

                // Copy file data
                reader.JumpTo(filePtr);
                byte[] bytes = reader.ReadBytes((int)fileSize);

                // Get the directory name information
                int entries = dirCount + fileCount;
                reader.JumpTo(nameTablePointer + (entries * 8));
                string[] fsData = reader.ReadNullTerminatedStringArray(entries);
                string dirName = fsData[dirID]; string fileName = fsData[dirCount + (nameID)];

                // Write everything to disk
                var newDir = baseDir + @"\" + dirName;
                Directory.CreateDirectory(newDir);


                double percent = (double)(i + 1) / (double)fileCount;
                int totalBlocks = 50;
                int progressBlocks = (int)(percent * totalBlocks);

                string bar = string.Format("Progress: {0}{1} {2,3}%  ({3} of {4} files extracted)\r",
                    new string('█', progressBlocks), new string('▒', totalBlocks - progressBlocks), (int)(percent * 100), i + 1, fileCount);

                Console.Write(bar);


                File.WriteAllBytes((newDir + @"\" + fileName), bytes);
                reader.JumpTo(lastIndexPos);
            }
        }


        /// <summary>
        /// Extracts Pre-Sega Forever versions of Sonic 4: Episode II OBB files.
        /// </summary>
        /// <param name="input">Takes in file path argument</param>
        /// <param name="baseDir">Takes in a base directory path for writing files</param>
        public static void ExtractLPKv1File(string input, string baseDir)
        {
            Stream stream = File.OpenRead(input);
            ExtendedBinaryReader reader = new ExtendedBinaryReader(stream);
            long lastIndexPos;

            reader.JumpTo(6);
            ushort fileCount = reader.ReadUInt16();
            ushort dirCount = reader.ReadUInt16();
            reader.JumpAhead(10);
            uint listPointer = reader.ReadUInt32();
            uint nameTablePointer = reader.ReadUInt32();
            uint nameTableLength = reader.ReadUInt32();
            reader.JumpTo(listPointer);

            for (int i = 0; i < fileCount; i++)
            {
                //Read the index entry.
                uint filePtr = reader.ReadUInt32();
                uint fileSize = reader.ReadUInt32();
                reader.JumpAhead(4);
                ushort dirID = reader.ReadUInt16();
                ushort nameID = reader.ReadUInt16();
                lastIndexPos = reader.BaseStream.Position;

                //copy file data
                reader.JumpTo(filePtr);
                byte[] bytes = reader.ReadBytes((int)fileSize);

                // Get the directory name information
                int entries = dirCount + fileCount;
                reader.JumpTo(nameTablePointer + (entries * 4));
                string[] fsData = reader.ReadNullTerminatedStringArray(entries);
                string dirName = fsData[dirID]; string fileName = fsData[dirCount + (nameID)];

                //write everything to disk
                var newDir = baseDir + @"\" + dirName;
                Directory.CreateDirectory(newDir);


                double percent = (double)(i + 1) / (double)fileCount;
                int totalBlocks = 50;
                int progressBlocks = (int)(percent * totalBlocks);

                string bar = string.Format("Progress: {0}{1} {2,3}%  ({3} of {4} files extracted)\r",
                    new string('█', progressBlocks), new string('▒', totalBlocks - progressBlocks), (int)(percent * 100), i + 1, fileCount);

                Console.Write(bar);


                File.WriteAllBytes((newDir + @"\" + fileName), bytes);
                reader.JumpTo(lastIndexPos);
            }
        }
        #endregion
    }
}
