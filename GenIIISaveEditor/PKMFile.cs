using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenIIISaveEditor
{

    internal class RawPKMData
    {
        public byte[] Data;

        public RawPKMData()
        {
            Data = new byte[100];
        }
    }

    internal class PKMData
    {
        public byte[] Data;

        public PKMData()
        {
            Data = new byte[12];
        }
    }
    
    internal class GrowthData : PKMData
    {
        public byte[] Species;
        public byte[] ItemHeld;
        public byte[] Experience;
        public byte[] PPBonuses;
        public byte[] Friendship;

        public GrowthData()
        {
            Species = new byte[2];
            ItemHeld = new byte[2];
            Experience = new byte[4];
            PPBonuses = new byte[1];
            Friendship = new byte[1];
        }
    }
    internal class PKMFile
    {
        public int AddressLocation;
        public byte[] PersonalityValue;
        public byte[] OTID;
        public byte[] Nickname;
        public byte[] Language;
        public byte[] EggName;
        public byte[] OTName;
        public byte[] Markings;
        public byte[] Checksum;
        public byte[] Data;
        public byte[] StatusCondition;
        public byte[] Level;
        public byte[] PokerusRemaining;
        public byte[] CurrentHP;
        public byte[] TotalHP;
        public byte[] Attack;
        public byte[] Defense;
        public byte[] Speed;
        public byte[] SpAttack;
        public byte[] SpDefense;

        public PKMFile()
        {
            AddressLocation = 0;
            PersonalityValue = new byte[4];
            OTID = new byte[4];
            Nickname = new byte[10];
            Language = new byte[1];
            EggName = new byte[1];
            OTName = new byte[7];
            Markings = new byte[1];
            Checksum = new byte[2];
            Data = new byte[48];
            StatusCondition = new byte[4];
            Level = new byte[1];
            PokerusRemaining = new byte[1];
            CurrentHP = new byte[2];
            TotalHP = new byte[2];
            Attack = new byte[2];
            Defense = new byte[2];
            Speed = new byte[2];
            SpAttack = new byte[2];
            SpDefense = new byte[2];
        }
    }
}
