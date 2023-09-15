using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Globalization;
using System.Buffers.Binary;
using System.Runtime.InteropServices;

namespace GenIIISaveEditor
{
    internal class Utilities
    {

        public static string[] SpeciesNames = new string[] { "Bulbasaur", "Ivysaur", "Venusaur", "Charmander", "Charmeleon", "Charizard", "Squirtle", "Wartortle", "Blastoise", "Caterpie", "Metapod", "Butterfree", "Weedle", "Kakuna", "Beedrill", "Pidgey", "Pidgeotto", "Pidgeot", "Rattata", "Raticate", "Spearow", "Fearow", "Ekans", "Arbok", "Pikachu", "Raichu", "Sandshrew", "Sandslash", "Nidoran (F)", "Nidorina", "Nidoqueen", "Nidoran (M)", "Nidorino", "Nidoking", "Clefairy", "Clefable", "Vulpix", "Ninetales", "Jigglypuff", "Wigglytuff", "Zubat", "Golbat", "Oddish", "Gloom", "Vileplume", "Paras", "Parasect", "Venonat", "Venomoth", "Diglett", "Dugtrio", "Meowth", "Persian", "Psyduck", "Golduck", "Mankey", "Primeape", "Growlithe", "Arcanine", "Poliwag", "Poliwhirl", "Poliwrath", "Abra", "Kadabra", "Alakazam", "Machop", "Machoke", "Machamp", "Bellsprout", "Weepinbell", "Victreebel", "Tentacool", "Tentacruel", "Geodude", "Graveler", "Golem", "Ponyta", "Rapidash", "Slowpoke", "Slowbro", "Magnemite", "Magneton", "Farfetch'd", "Doduo", "Dodrio", "Seel", "Dewgong", "Grimer", "Muk", "Shellder", "Cloyster", "Gastly", "Haunter", "Gengar", "Onix", "Drowzee", "Hypno", "Krabby", "Kingler", "Voltorb", "Electrode", "Exeggcute", "Exeggutor", "Cubone", "Marowak", "Hitmonlee", "Hitmonchan", "Lickitung", "Koffing", "Weezing", "Rhyhorn", "Rhydon", "Chansey", "Tangela", "Kangaskhan", "Horsea", "Seadra", "Goldeen", "Seaking", "Staryu", "Starmie", "Mr. Mime", "Scyther", "Jynx", "Electabuzz", "Magmar", "Pinsir", "Tauros", "Magikarp", "Gyarados", "Lapras", "Ditto", "Eevee", "Vaporeon", "Jolteon", "Flareon", "Porygon", "Omanyte", "Omastar", "Kabuto", "Kabutops", "Aerodactyl", "Snorlax", "Articuno", "Zapdos", "Moltres", "Dratini", "Dragonair", "Dragonite", "Mewtwo", "Mew", "Chikorita", "Bayleef", "Meganium", "Cyndaquil", "Quilava", "Typhlosion", "Totodile", "Croconaw", "Feraligatr", "Sentret", "Furret", "Hoothoot", "Noctowl", "Ledyba", "Ledian", "Spinarak", "Ariados", "Crobat", "Chinchou", "Lanturn", "Pichu", "Cleffa", "Igglybuff", "Togepi", "Togetic", "Natu", "Xatu", "Mareep", "Flaaffy", "Ampharos", "Bellossom", "Marill", "Azumarill", "Sudowoodo", "Politoed", "Hoppip", "Skiploom", "Jumpluff", "Aipom", "Sunkern", "Sunflora", "Yanma", "Wooper", "Quagsire", "Espeon", "Umbreon", "Murkrow", "Slowking", "Misdreavus", "Unown", "Wobbuffet", "Girafarig", "Pineco", "Forretress", "Dunsparce", "Gligar", "Steelix", "Snubbull", "Granbull", "Qwilfish", "Scizor", "Shuckle", "Heracross", "Sneasel", "Teddiursa", "Ursaring", "Slugma", "Magcargo", "Swinub", "Piloswine", "Corsola", "Remoraid", "Octillery", "Delibird", "Mantine", "Skarmory", "Houndour", "Houndoom", "Kingdra", "Phanpy", "Donphan", "Porygon2", "Stantler", "Smeargle", "Tyrogue", "Hitmontop", "Smoochum", "Elekid", "Magby", "Miltank", "Blissey", "Raikou", "Entei", "Suicune", "Larvitar", "Pupitar", "Tyranitar", "Lugia", "Ho-Oh", "Celebi", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "Treecko", "Grovyle", "Sceptile", "Torchic", "Combusken", "Blaziken", "Mudkip", "Marshtomp", "Swampert", "Poochyena", "Mightyena", "Zigzagoon", "Linoone", "Wurmple", "Silcoon", "Beautifly", "Cascoon", "Dustox", "Lotad", "Lombre", "Ludicolo", "Seedot", "Nuzleaf", "Shiftry", "Nincada", "Ninjask", "Shedinja", "Taillow", "Swellow", "Shroomish", "Breloom", "Spinda", "Wingull", "Pelipper", "Surskit", "Masquerain", "Wailmer", "Wailord", "Skitty", "Delcatty", "Kecleon", "Baltoy", "Claydol", "Nosepass", "Torkoal", "Sableye", "Barboach", "Whiscash", "Luvdisc", "Corphish", "Crawdaunt", "Feebas", "Milotic", "Carvanha", "Sharpedo", "Trapinch", "Vibrava", "Flygon", "Makuhita", "Hariyama", "Electrike", "Manectric", "Numel", "Camerupt", "Spheal", "Sealeo", "Walrein", "Cacnea", "Cacturne", "Snorunt", "Glalie", "Lunatone", "Solrock", "Azurill", "Spoink", "Grumpig", "Plusle", "Minun", "Mawile", "Meditite", "Medicham", "Swablu", "Altaria", "Wynaut", "Duskull", "Dusclops", "Roselia", "Slakoth", "Vigoroth", "Slaking", "Gulpin", "Swalot", "Tropius", "Whismur", "Loudred", "Exploud", "Clamperl", "Huntail", "Gorebyss", "Absol", "Shuppet", "Banette", "Seviper", "Zangoose", "Relicanth", "Aron", "Lairon", "Aggron", "Castform", "Volbeat", "Illumise", "Lileep", "Cradily", "Anorith", "Armaldo", "Ralts", "Kirlia", "Gardevoir", "Bagon", "Shelgon", "Salamence", "Beldum", "Metang", "Metagross", "Regirock", "Regice", "Registeel", "Kyogre", "Groudon", "Rayquaza", "Latias", "Latios", "Jirachi", "Deoxys", "Chimecho", "Pok\u00e9mon Egg", "Unown", "Unown", "Unown", "Unown", "Unown", "Unown", "Unown", "Unown", "Unown", "Unown", "Unown", "Unown", "Unown", "Unown", "Unown", "Unown", "Unown", "Unown", "Unown", "Unown", "Unown", "Unown", "Unown", "Unown", "Unown", "Unown", "Unown" };

        public static string[] byteNames = new string[83] {"A1", "A2", "A3", "A4", "A5", "A6", "A7", "A8", "A9", "AA", "AB", "AC", "AD", "AE", "B0", "B1", "B2", "B3", "B4", "B5", "B6", "B8", "BA", "BB", "BC", "BD",
    "BE", "BF", "C0", "C1", "C2", "C3", "C4", "C5", "C6", "C7", "C8", "C9", "CA", "CB", "CC", "CD", "CE", "CF", "D0", "D1", "D2", "D3", "D4", "D5", "D6", "D7", "D8", "D9", "DA", "DB", "DC", "DD", "DE", "DF",
"E0", "E1", "E2", "E3", "E4", "E5", "E6", "E7", "E8", "E9", "EA", "EB", "EC", "ED", "EE", "F1", "F2", "F3", "F4", "F5", "F6", "FF", "00"};
        public static string[] characterNames = new string[83] { "0", "1", "2", "3", "4", "5", "6", "7", "8", "9", "!", "?", ".", "-", "...", "“", "”", "‘", "’", "♂", "♀", ",", "/", "A", "B", "C", "D", "E", "F", "G"
, "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z", "a", "b", "c", "d", "e", "f", "g", "h", "i", "j", "k", "l", "m", "n", "o", "p", "q", "r", "s", "t",
"u", "v", "w", "x", "y", "z", "Ä", "Ö", "Ü", "ä", "ö", "ü", "", " "};

        public UIntPtr ARRAY_COUNT(byte[] array)
        {
            return (UIntPtr)(array.Length / sizeof(byte));
        }

        public static bool ReadNBit(uint data, int bit)
        {
            return (data & (1 << bit - 1)) != 0;
        }

        public static void __decryptsubsection(ref byte[] data, UInt32 otID, UInt32 personalityValue)
        {
            if (data.Length < 12)
                return;

            var decryptionKey = otID ^ personalityValue;
            UInt32 intData = BitConverter.ToUInt32(data);

            for (int i = 0; i < 12; i++)
            {
                intData ^= otID;
                intData ^= personalityValue;
            }

            data = BitConverter.GetBytes(intData);
        }

        public static byte[] exlusiveOR(byte[] array, uint key)
        {
            byte[] result = new byte[array.Length];

            for (int i = 0; i < array.Length; ++i)
            {
                var b = array[i];
                var xored = Convert.ToUInt32(b) ^ key;
                result[i] = Convert.ToByte(xored);
            }

            return result;
        }

        public static bool AreAllChecksumsValid(SaveFile usedFile)
        {
            foreach (Section section in usedFile.Sections)
            {
                if (section == null) continue;
                var checksum = BitConverter.ToUInt16(section.Checksum);
                var newChecksum = section.GetChecksumForData(section.Data.Data, GetSectionSize(BitConverter.ToUInt16(section.SectionID)));

                if (checksum != newChecksum)
                    return false;
            }

            return true;
        }

        public static ReadOnlySpan<byte> BlockPosition => new byte[]
        {
            0, 1, 2, 3,
            0, 1, 3, 2,
            0, 2, 1, 3,
            0, 3, 1, 2,
            0, 2, 3, 1,
            0, 3, 2, 1,
            1, 0, 2, 3,
            1, 0, 3, 2,
            2, 0, 1, 3,
            3, 0, 1, 2,
            2, 0, 3, 1,
            3, 0, 2, 1,
            1, 2, 0, 3,
            1, 3, 0, 2,
            2, 1, 0, 3,
            3, 1, 0, 2,
            2, 3, 0, 1,
            3, 2, 0, 1,
            1, 2, 3, 0,
            1, 3, 2, 0,
            2, 1, 3, 0,
            3, 1, 2, 0,
            2, 3, 1, 0,
            3, 2, 1, 0,

        // duplicates of 0-7 to eliminate modulus
            0, 1, 2, 3,
            0, 1, 3, 2,
            0, 2, 1, 3,
            0, 3, 1, 2,
            0, 2, 3, 1,
            0, 3, 2, 1,
            1, 0, 2, 3,
            1, 0, 3, 2,
        };


        public static void CryptArray3(Span<byte> ekm, uint seed)
        {
            if (!BitConverter.IsLittleEndian)
                seed = BinaryPrimitives.ReverseEndianness(seed);

            var toEncrypt = ekm[32..80];

            foreach (ref var u32 in MemoryMarshal.Cast<byte, uint>(toEncrypt))
                u32 ^= seed;
        }

        public static byte[] ShuffleArray3(ReadOnlySpan<byte> data, uint sv)
        {
            byte[] sdata = new byte[data.Length];
            ShuffleArray3(data, sdata, sv);
            return sdata;      
        }

        public static void ShuffleArray3(ReadOnlySpan<byte> data, Span<byte> result, uint sv)
        {
            int index = (int)sv * 4;
            data[..32].CopyTo(result[..32]);
            data[80..].CopyTo(result[80..]);
            for (int block = 3; block >= 0; block--)
            {
                var dest = result.Slice(32 + (12 * block), 12);
                int ofs = BlockPosition[index + block];
                var src = data.Slice(32 + (12 * ofs), 12);
                src.CopyTo(dest);
            }
        }

        public static byte[] DecryptArray3(Span<byte> ekm)
        {
            uint PID = BinaryPrimitives.ReadUInt32BigEndian(ekm);
            uint OID = BinaryPrimitives.ReadUInt32BigEndian(ekm[4..]);
            uint seed = PID ^ OID;
            CryptArray3(ekm, seed);
            return ShuffleArray3(ekm, PID % 24);
        }

        

        public static string GetSpeciesName(UInt16 species)
        {
            return SpeciesNames[species];
        }

        public static string GetSubstructureOrder(UInt32 PersonalityValue)
        {
            //[0, 1, 2, 3] = GAEM
            //G = 0
            //A = 1
            //M = 3
            //E = 2

            UInt32 order = PersonalityValue % 24;

            switch (order)
            {
                case 0: return "GAEM";
                case 1: return "GAME";
                case 2: return "GEAM";
                case 3: return "GEMA";
                case 4: return "GMAE";
                case 5: return "GMEA";
                case 6: return "AGEM";
                case 7: return "AGME";
                case 8: return "AEGM";
                case 9: return "AEMG";
                case 10: return "AMGE";
                case 11: return "AMEG";
                case 12: return "EGAM";
                case 13: return "EGMA";
                case 14: return "EAGM";
                case 15: return "EAMG";
                case 16: return "EMGA";
                case 17: return "EMAG";
                case 18: return "MGAE";
                case 19: return "MGEA";
                case 20: return "MAGE";
                case 21: return "MAEG";
                case 22: return "MEGA";
                case 23: return "MEAG";
            }

            return "";
        }
        
        public static byte[] EncryptPlaytime(PlayTime playTime)
        {
            byte[] returnValue = new byte[5];
            byte[] hours = BitConverter.GetBytes(playTime.Hours);
            returnValue[0] = hours[0];
            returnValue[1] = hours[1];

            returnValue[2] = (byte)playTime.Minutes;
            returnValue[3] = (byte)playTime.Seconds;
            returnValue[4] = (byte)playTime.Frames;


            return returnValue;
        }
        public static PlayTime GetPlayTime(byte[] playtimeData)
        {
            PlayTime playTime = new PlayTime();

            playTime.Hours = BitConverter.ToUInt16(new byte[2] { playtimeData[0], playtimeData[1] });
            playTime.Minutes = Convert.ToSByte(playtimeData[2]);
            playTime.Seconds = Convert.ToSByte(playtimeData[3]);
            playTime.Frames = Convert.ToSByte(playtimeData[4]);

            return playTime;
        }


        public static bool ParseHex(string inHex, out byte value)
        {
            if (string.IsNullOrEmpty(inHex))
            {
                value = 0;
                return false;
            }

            if (inHex.StartsWith("0x") || inHex.StartsWith("0X"))
                return byte.TryParse(inHex.Substring(2), NumberStyles.AllowHexSpecifier, null, out value);
            else
                return byte.TryParse(inHex, NumberStyles.Integer, CultureInfo.InvariantCulture, out value);
        }

        public static string GetDecryptedCharacter(byte encryptedCharacter)
        {
            string byteName = encryptedCharacter.ToString("X");

            var index = Array.IndexOf(byteNames, byteName);

            if (index > -1)
            {
                return characterNames[index];
            }

            return "";
        }




        public static byte[] EncryptString(string DecryptedString, int totalLength)
        {
            byte[] returnValue = new byte[totalLength];

            for (int i = 0; i < DecryptedString.Length; i++)
            {
                char character = DecryptedString[i];
                var stringVal = character.ToString();

                if (Array.IndexOf(characterNames, stringVal) == -1)
                    continue;

                var byteToUse = byteNames[Array.IndexOf(characterNames, stringVal)];
                byteToUse = "0x" + byteToUse;
                byte byteVal;
                ParseHex(byteToUse, out byteVal);
                returnValue[i] = byteVal;
            }

            var tempList = returnValue.ToList();
            for (int i = 0; i < totalLength - DecryptedString.Length; i++)
            {
                byte terminator;
                ParseHex("0xFF", out terminator);
                tempList.Add(terminator);
            }
            return tempList.ToArray();
        }

        public static string DecryptMessage(byte[] encryptedString)
        {
            string finalString = "";
            foreach (var b in encryptedString)
            {
                if (b.ToString("X") == "FF")
                    break;

                var character = GetDecryptedCharacter(b);
                finalString += character;
            }

            return finalString;
        }

        public static string GetEncryptedBytes(byte[] encryptedString)
        {
            string finalString = "";
            foreach (var b in encryptedString)
            {
                finalString += b.ToString("X");
            }

            return finalString;
        }

        public static UInt16 GetSectionID(string Name)
        {
            switch (Name)
            {
                case "Trainer Info":
                    return 0;
                case "Team / Items":
                    return 1;
                case "Game State":
                    return 2;
                case "Misc Data":
                    return 3;
                case "Rival Info":
                    return 4;
                case "PC buffer A":
                    return 5;
                case "PC buffer B":
                    return 6;
                case "PC buffer C":
                    return 7;
                case "PC buffer D":
                    return 8;
                case "PC buffer E":
                    return 9;
                case "PC buffer F":
                    return 10;
                case "PC buffer G":
                    return 11;
                case "PC buffer H":
                    return 12;
                case "PC buffer I":
                    return 13;
                default:
                    return 0;
            }
        }

        public static string GetSectionName(UInt16 SectionID)
        {
            switch (SectionID)
            {
                case 0:
                    return "Trainer Info";
                case 1:
                    return "Team / Items";
                case 2:
                    return "Game State";
                case 3:
                    return "Misc Data";
                case 4:
                    return "Rival Info";
                case 5:
                    return "PC buffer A";
                case 6:
                    return "PC buffer B";
                case 7:
                    return "PC buffer C";
                case 8:
                    return "PC buffer D";
                case 9:
                    return "PC buffer E";
                case 10:
                    return "PC buffer F";
                case 11:
                    return "PC buffer G";
                case 12:
                    return "PC buffer H";
                case 13:
                    return "PC buffer I";
                default:
                    return "";
            }
        }
        public static int GetSectionSize(UInt16 SectionID)
        {
            switch (SectionID)
            {
                case 0:
                    return 3884;
                case 1:
                    return 3968;
                case 2:
                    return 3968;
                case 3:
                    return 3968;
                case 4:
                    return 3848;
                case 5:
                    return 3968;
                case 6:
                    return 3968;
                case 7:
                    return 3968;
                case 8:
                    return 3968;
                case 9:
                    return 3968;
                case 10:
                    return 3968;
                case 11:
                    return 3968;
                case 12:
                    return 3968;
                case 13:
                    return 2000;
                default:
                    return 0;
            }
        }
        
        public static void WriteToSection(string fileName, int SectionOffset, int size, byte[] toWrite, int propertyOffset)
        {
            using (FileStream fsSource = new FileStream(fileName, FileMode.Open, FileAccess.ReadWrite))
            using (BinaryWriter binaryWriter = new BinaryWriter(fsSource))
            {
                fsSource.Seek(SectionOffset, SeekOrigin.Begin);
                if (propertyOffset > 0)
                    fsSource.Seek(propertyOffset, SeekOrigin.Current);
                fsSource.Write(toWrite, 0, size);
            }
        }
    }
}
