using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenIIISaveEditor
{
    
    public class TrainerID
    {
        public UInt32 TID;
        public UInt32 SID;

        public TrainerID()
        {
            TID = 0;
            SID = 0;
        }
    }

    
    public class PatchInfo
    {
        public int Position;
        public int Size;
        public byte[] PatchData;
        public int Offset;

        public PatchInfo(int DataSize)
        {
            Position = 0x0;
            Size = DataSize;
            PatchData = new byte[Size];
            Offset = 0x0;
        }
    }
    public class SaveFile
    {
        public Section[] Sections;

        public SaveFile()
        {
            Sections = new Section[14];
        }

        public Section GetByID(UInt16 sectionId)
        {
            foreach (Section section in Sections)
            {
                if (BitConverter.ToUInt16(section.SectionID) == sectionId)
                    return section;
            }
            
            return new Section();
        }
    }

    public class PlayTime
    {
        public UInt16 Hours;
        public sbyte Minutes;
        public sbyte Seconds;
        public sbyte Frames;

        public PlayTime()
        {
            Hours = 0;
            Minutes = 0;
            Seconds = 0;
            Frames = 0;
        }
    }

    public class SectionData
    {
        public byte[] Data;

        public SectionData()
        {
            Data = new byte[3968];
        }
    }

    public class TeamAndItems : SectionData
    {
        public byte[] TeamSize;
        public byte[] TeamPokemonList;
        public byte[] Money;
        public byte[] Coins;
        public byte[] PCItems;
        public byte[] ItemPocket;
        public byte[] KeyItemPocket;
        public byte[] BallItemPocket;
        public byte[] TMCase;
        public byte[] BerryPocket;

        public TeamAndItems()
        {
            TeamSize = new byte[4];
            TeamPokemonList = new byte[600];
            Money = new byte[4];
            Coins = new byte[2];
            PCItems = new byte[120];
            ItemPocket = new byte[168];
            KeyItemPocket = new byte[120];
            BallItemPocket = new byte[52];
            TMCase = new byte[232];
            BerryPocket = new byte[172];
        }
    }

    public class TrainerInfo : SectionData
    {
        public byte[] playerName;
        public byte[] playerGender;
        public byte[] TrainerID;
        public byte[] TimePlayed;
        public byte[] Options;
        public byte[] GameCode;
        public byte[] SecurityKey;

        public TrainerInfo()
        {
            playerName = new byte[7];
            playerGender = new byte[1];
            TrainerID = new byte[4];
            TimePlayed = new byte[5];
            Options = new byte[3];
            GameCode = new byte[4];
            SecurityKey = new byte[4];
        }
    }

    public class Section
    {
        public int SectionPosition;
        public SectionData Data;
        public byte[] SectionID;
        public byte[] Checksum;
        public byte[] Signature;
        public byte[] SaveIndex;

        public Section()
        {
            SectionPosition = 0;
            Data = new SectionData();
            SectionID = new byte[2];
            Checksum = new byte[2];
            Signature = new byte[4];
            SaveIndex = new byte[4];
        }

        public UInt16 GetChecksumForData(byte[] data, int size)
        {
            UInt16 i;
            UInt32 checksum = 0;

            for (i = 0; i < (size / 4); i++)
            {
                checksum += BitConverter.ToUInt32(data, i * 4);
            }

            return (UInt16)((checksum >> 16) + checksum);
        }

    }
}
