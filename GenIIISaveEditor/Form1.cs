
namespace GenIIISaveEditor
{
    public partial class Form1 : Form
    {
        //Save files used to initialize sections - NOT used in code, use "usedFile" below
        SaveFile fileA = new SaveFile();
        SaveFile fileB = new SaveFile();

        //Object used in code, selected by save index
        SaveFile usedFile = new SaveFile();


        //Number of save sections
        int NumSections = 14;

        //HACK - External section data definitions
        TeamAndItems teamAndItems = new TeamAndItems();
        TrainerInfo TrainerInfo = new TrainerInfo();

        //HACK - Store all pokemon externally
        List<PKMFile> Pokemons = new List<PKMFile>();

        //section data
        public static long SECTION_DATA = 0x00;
        public static long SECTION_SECTIONID = 0x0FF4;
        public static long SECTION_CHECKSUM = 0x0FF6;
        public static long SECTION_SIGNATURE = 0x0FF8;
        public static long SECTION_SAVEINDEX = 0x0FFC;
        //trainer info
        public static long TRAINERINFO_PLAYERNAME = 0x0000;
        public static long TRAINERINFO_PLAYERGENDER = 0x0008;
        public static long TRAINERINFO_TRAINERID = 0x000A;
        public static long TRAINERINFO_TIMEPLAYED = 0x000E;
        public static long TRAINERINFO_OPTIONS = 0x0013;
        public static long TRAINERINFO_SECURITYKEY = 0x0F20;

        //team and items
        public static long TEAMANDITEMS_TEAMSIZE = 0x0034;

        //team pokemon offsets
        public static long TEAMPKMN_1 = 0x0038;
        public static long TEAMPKMN_2 = 0x9C;
        public static long TEAMPKMN_3 = 0x100;
        public static long TEAMPKMN_4 = 0x164;
        public static long TEAMPKMN_5 = 0x1C8;
        public static long TEAMPKMN_6 = 0x22C;

        public static long TEAMANDITEMS_MONEY = 0x0290;

        //pokemon data
        public static long POKEMON_PERSONALITYVALUE = 0x00;
        public static long POKEMON_OTID = 0x4;
        public static long POKEMON_NICKNAME = 0x8;
        public static long POKEMON_OTNAME = 0x14;
        public static long POKEMON_DATA = 0x20;
        public static long POKEMON_STATUSCONDITION = 0x50;
        public static long POKEMON_LEVEL = 0x54;
        public static long POKEMON_CURRENTHP = 0x56;
        public static long POKEMON_TOTALHP = 0x58;

        public static string FileName = "";

        public Form1()
        {
            InitializeComponent();
        }

        public enum ELogType
        {
            Info,
            Warning,
            Error,
            Fatal,
            Debug
        };

        
        public void ConsoleLog(ELogType logType, string message)
        {
            Color color = Color.Black;
            switch (logType)
            {
                case ELogType.Warning:
                    color = Color.Orange;
                    break;
                case ELogType.Error:
                    color = Color.Red;
                    break;
                case ELogType.Fatal:
                    color = Color.DarkRed;
                    break;
            }
            string finalText = $"[{DateTime.Now}] " + message + "\n";
#if DEBUG
            if (logType == ELogType.Debug)
            {
                color = Color.DarkBlue;
                finalText = "[DEBUG] " + finalText;
            }
#endif

#if RELEASE
            if (logType == ELogType.Debug)
                return;
#endif
            richTextBox1.AppendText(finalText, color);
            richTextBox1.ScrollToCaret();
        }

        public TrainerID GetTrainerID(byte[] TrainerIDData)
        {
            TrainerID trainerID = new TrainerID();

            UInt32 idFull = BitConverter.ToUInt32(TrainerIDData);

            trainerID.TID = (idFull & 0xFFFF);
            trainerID.SID = (idFull >> 16);

            return trainerID;
        }


        public void ConsoleLog(Color color, string message)
        {
            string finalText = $"[{DateTime.Now}] " + message + "\n";

            richTextBox1.AppendText(finalText, color);
            richTextBox1.ScrollToCaret();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            ConsoleLog(ELogType.Info, "Program initialized successfully!");
            TrainerTab.Enabled = false;
            var settings = Utilities.GetSettings();

            for (int i = 0; i < 4; i++)
            {
                var setting = settings.Read($"File{i + 1}", "RecentFiles");
                ConsoleLog(ELogType.Debug, $"Got Setting {setting} for key File{i+1}");

                switch (i)
                {
                    case 0:
                        toolStripMenuItem2.Text = "1. " + setting;
                        break;
                    case 1:
                        toolStripMenuItem3.Text = "2. " + setting;
                        break;
                    case 2:
                        toolStripMenuItem4.Text = "3. " + setting;
                        break;
                    case 3:
                        toolStripMenuItem5.Text = "4. " + setting;
                        break;
                }
            }
        }

        private void Form1_OnFormClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (usedFile.Sections.Length > 0)
            {
                if (!Utilities.AreAllChecksumsValid(usedFile))
                {
                    if (MessageBox.Show("Are you sure you want to exit? 1 or more checksums are invalid.", "Warning", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.No)
                    {
                        e.Cancel = true;
                    }
                }
            }
        }

        private void SetupTrainerInfo()
        {
            TrainerTab.Enabled = true;
            PokemonCombo.Items.Clear();
            PokemonCombo.SelectedIndex = -1;
            trainerGroup.Visible = false;
            pokemonGroup.Visible = false;
            statusGroup.Visible = false;
            TIDLabel.Visible = PokemonTIDBox.Visible = false;
            SIDLabel.Visible = PokemonSIDBox.Visible = false;
            PokemonTrainerNameBox.Visible = TrainerNameLabel.Visible = false;
            NicknameLabel.Visible = PokemonNicknameBox.Visible = false;
            LevelLabel.Visible = LevelBox.Visible = false;
            HPLabel.Visible = CurHpBox.Visible = MaxHpBox.Visible = false;
            sleepChk.Visible = sleepTurnsBox.Visible = sleepTurnsLabel.Visible = false;
            poisonChk.Visible = paralzChk.Visible = freezeChk.Visible = burnChk.Visible = badpoisonChk.Visible = false;

            Section trainerSection = usedFile.GetByID(0);
            Section itemsSection = usedFile.GetByID(1);

            using (MemoryStream memoryStream = new MemoryStream(trainerSection.Data.Data))
            using (BinaryReader binaryReader = new BinaryReader(memoryStream))
            {
                
                memoryStream.Read(TrainerInfo.playerName, 0, 7);
                memoryStream.Seek(TRAINERINFO_PLAYERGENDER, SeekOrigin.Begin);
                memoryStream.Read(TrainerInfo.playerGender, 0, 1);
                memoryStream.Seek(TRAINERINFO_TRAINERID, SeekOrigin.Begin);
                memoryStream.Read(TrainerInfo.TrainerID, 0, 4);
                memoryStream.Seek(TRAINERINFO_TIMEPLAYED, SeekOrigin.Begin);
                memoryStream.Read(TrainerInfo.TimePlayed, 0, 5);
                memoryStream.Seek(TRAINERINFO_OPTIONS, SeekOrigin.Begin);
                memoryStream.Read(TrainerInfo.Options, 0, 3);
                memoryStream.Seek(TRAINERINFO_SECURITYKEY, SeekOrigin.Begin);
                memoryStream.Read(TrainerInfo.SecurityKey, 0, 4);
            }

            RawPKMData[] pokemonData = new RawPKMData[6];

            using (MemoryStream memoryStream = new MemoryStream(itemsSection.Data.Data))
            using (BinaryReader binaryReader = new BinaryReader(memoryStream))
            {
                memoryStream.Seek(TEAMANDITEMS_TEAMSIZE, SeekOrigin.Begin);
                memoryStream.Read(teamAndItems.TeamSize, 0, 4);

                
                memoryStream.Seek(TEAMPKMN_1, SeekOrigin.Begin);
                RawPKMData newData = new RawPKMData();
                memoryStream.Read(newData.Data, 0, 100);
                pokemonData[0] = newData;

                newData = new RawPKMData();
                memoryStream.Seek(TEAMPKMN_2, SeekOrigin.Begin);
                memoryStream.Read(newData.Data, 0, 100);
                pokemonData[1] = newData;

                newData = new RawPKMData();
                memoryStream.Seek(TEAMPKMN_3, SeekOrigin.Begin);
                memoryStream.Read(newData.Data, 0, 100);
                pokemonData[2] = newData;

                newData = new RawPKMData();
                memoryStream.Seek(TEAMPKMN_4, SeekOrigin.Begin);
                memoryStream.Read(newData.Data, 0, 100);
                pokemonData[3] = newData;

                newData = new RawPKMData();
                memoryStream.Seek(TEAMPKMN_5, SeekOrigin.Begin);
                memoryStream.Read(newData.Data, 0, 100);
                pokemonData[4] = newData;

                newData = new RawPKMData();
                memoryStream.Seek(TEAMPKMN_6, SeekOrigin.Begin);
                memoryStream.Read(newData.Data, 0, 100);
                pokemonData[5] = newData;

                memoryStream.Seek(TEAMANDITEMS_MONEY, SeekOrigin.Begin);
                memoryStream.Read(teamAndItems.Money, 0, 4);
            }

            foreach (RawPKMData firstPokemon in pokemonData)
            {

                using (MemoryStream memoryStream = new MemoryStream(firstPokemon.Data))
                using (BinaryReader binaryReader = new BinaryReader(memoryStream))
                {
                    PKMFile Pokemon = new PKMFile();

                    memoryStream.Read(Pokemon.PersonalityValue, 0, 4);
                    memoryStream.Seek(POKEMON_OTID, SeekOrigin.Begin);
                    memoryStream.Read(Pokemon.OTID, 0, 4);
                    memoryStream.Seek(POKEMON_NICKNAME, SeekOrigin.Begin);
                    memoryStream.Read(Pokemon.Nickname, 0, 10);
                    memoryStream.Seek(POKEMON_OTNAME, SeekOrigin.Begin);
                    memoryStream.Read(Pokemon.OTName, 0, 7);
                    memoryStream.Seek(POKEMON_DATA, SeekOrigin.Begin);
                    memoryStream.Read(Pokemon.Data, 0, 48);
                    memoryStream.Seek(POKEMON_STATUSCONDITION, SeekOrigin.Begin);
                    memoryStream.Read(Pokemon.StatusCondition, 0, 4);
                    memoryStream.Seek(POKEMON_LEVEL, SeekOrigin.Begin);
                    memoryStream.Read(Pokemon.Level, 0, 1);
                    memoryStream.Seek(POKEMON_CURRENTHP, SeekOrigin.Begin);
                    memoryStream.Read(Pokemon.CurrentHP, 0, 2);
                    memoryStream.Seek(POKEMON_TOTALHP, SeekOrigin.Begin);
                    memoryStream.Read(Pokemon.TotalHP, 0, 2);

                    Pokemons.Add(Pokemon);
                }
            }

            /*foreach (PKMFile pokemon in Pokemons)
            {
                var orders = new string[] { "GAEM", "GAME", "GEAM", "GEMA", "GMAE", "GMEA", "AGEM", "AGME", "AEGM", "AEMG", "AMGE", "AMEG", "EGAM", "EGMA", "EAGM", "EAMG", "EMGA", "EMAG", "MGAE", "MGEA", "MAGE", "MAEG", "MEGA", "MEAG" };
                
                var personalityValue = BitConverter.ToUInt32(pokemon.PersonalityValue);
                var order = personalityValue % 24;
                var orderstring = orders[order];
                var growthIndex = orderstring.IndexOf('G');
                ConsoleLog(ELogType.Debug, orderstring);
                //ConsoleLog(ELogType.Debug, growthIndex.ToString());
                var TID = GetTrainerID(pokemon.OTID).TID;
                var personality = BitConverter.ToUInt32(pokemon.PersonalityValue);
                var key = TID ^ personality;
                var sections = new PKMData[4];

                

                var section = orderstring[growthIndex];
                var species = pokemon.Data.Skip(12).Take(2);
                ConsoleLog(ELogType.Debug, Convert.ToUInt16(species).ToString());
                //var decr = Utilities.__decryptsubsection(sectionData, TID, personalityValue);
               

                using (MemoryStream memStream = new MemoryStream(sections[growthIndex].Data))
                {
                    GrowthData pokemonGrowth = new GrowthData();
                    memStream.Read(pokemonGrowth.Species, 0, 2);
                    UInt16 pokemonSpecies = BitConverter.ToUInt16(pokemonGrowth.Species);
                    ConsoleLog(ELogType.Debug, pokemonSpecies.ToString());
                }


            } */

            var data = new byte[12];
            var otid = BitConverter.ToUInt32(Pokemons[0].OTID);
            var personalityValue = BitConverter.ToUInt32(Pokemons[0].PersonalityValue);

            var subsection = Utilities.GetSubstructureOrder(personalityValue);
            byte[] species = new byte[2];

            using (MemoryStream memStream = new MemoryStream(Pokemons[0].Data))
            {
                memStream.Seek(0 * subsection.IndexOf('G'), SeekOrigin.Begin);
                memStream.Read(data, 0, 12);
            }

            var decrypted = Utilities.__decryptsubsection(data, otid, personalityValue);


            using (MemoryStream memStream = new MemoryStream(decrypted))
            {
                memStream.Read(species, 0, 2);
            }

            UInt16 specis = BitConverter.ToUInt16(species);
            ConsoleLog(ELogType.Debug, specis.ToString());


            foreach (PKMFile pokemon in Pokemons)
            {
                PokemonCombo.Items.Add(Utilities.DecryptMessage(pokemon.Nickname));
            }

                PlayerNameBox.Text = Utilities.DecryptMessage(TrainerInfo.playerName);

            if (TrainerInfo.playerGender[0].ToString("X") == "0")
                GenderBox.SelectedIndex = 0;
            else
                GenderBox.SelectedIndex = 1;

            UInt32 idFull = BitConverter.ToUInt32(TrainerInfo.TrainerID);
            TIDBox.Text = (idFull & 0xFFFF).ToString();
            SIDBox.Text = (idFull >> 16).ToString();

            MoneyBox.Text = (BitConverter.ToUInt32(teamAndItems.Money) ^ BitConverter.ToUInt32(TrainerInfo.SecurityKey)).ToString();

            PlayTime playTime = Utilities.GetPlayTime(TrainerInfo.TimePlayed);
            HoursBox.Text = playTime.Hours.ToString();
            MinutesBox.Text = playTime.Minutes.ToString();
            SecondsBox.Text = playTime.Seconds.ToString();

            TeamSize.Text = BitConverter.ToUInt32(teamAndItems.TeamSize).ToString();

        }

        private void OnFileSystemInitialized(SaveFile fileToUse)
        {
            ChecksumSections.Items.Clear();
            PokemonCombo.Items.Clear();
            ChecksumSections.SelectedIndex = -1;
            ChecksumSections.Text = "";
            SectionText.Visible = false;
            CheckChecksums.Visible = false;
            ChecksumValue.Visible = false;
            ChecksumLabel.Visible = false;
            button2.Visible = false;
            button3.Visible = false;

            foreach (var section in fileToUse.Sections)
            {
                ChecksumSections.Items.Add(Utilities.GetSectionName(BitConverter.ToUInt16(section.SectionID)));
            }

            ChecksumSections.Visible = true;
            SectionText.Visible = true;
            CheckChecksums.Visible = true;
            toolTip1.SetToolTip(ChecksumErrorImage, "Checksum value invalid");

            sectionLocationOffset.Visible = false;

#if DEBUG
            sectionLocationLabel.Visible = true;
#endif
            SetupTrainerInfo();
        }

        private void InitializeFileSystem(string fileName)
        {
            FileName = fileName;
            Array.Clear(fileA.Sections);
            Array.Clear(fileB.Sections);
            Pokemons.Clear();
            PokemonCombo.Items.Clear();
            ConsoleLog(ELogType.Info, $"Initializing FileSystem for file {openFileDialog1.SafeFileName}");
            try
            {
                Utilities.UpdateRecentFiles(Utilities.GetSettings(), fileName);

                var settings = Utilities.GetSettings();

                for (int i = 0; i < 4; i++)
                {
                    var setting = settings.Read($"File{i + 1}", "RecentFiles");
                    ConsoleLog(ELogType.Debug, $"Got Setting {setting} for key File{i + 1}");

                    switch (i)
                    {
                        case 0:
                            toolStripMenuItem2.Text = "1. " + setting;
                            break;
                        case 1:
                            toolStripMenuItem3.Text = "2. " + setting;
                            break;
                        case 2:
                            toolStripMenuItem4.Text = "3. " + setting;
                            break;
                        case 3:
                            toolStripMenuItem5.Text = "4. " + setting;
                            break;
                    }
                }

                //Initialize FileStream with save file as source, Open file and Read contents
                using (FileStream fsSource = new FileStream(FileName, FileMode.Open, FileAccess.Read))
                using (BinaryReader binaryReader = new BinaryReader(fsSource)) //Initialize BinaryReader with fsSource as Stream
                {
                    ConsoleLog(ELogType.Info, "Initialized FileSystem successfully.");
                    //Set SaveFile label to file name
                    

                    for (int i = 0; i < NumSections; i++)
                    {
                        Section section = new Section();
                        var BasePosition = fsSource.Position;

                        section.SectionPosition = (int)BasePosition;


                        //Read main section data into section object
                        fsSource.Read(section.Data.Data, 0, 3968);

                        //Read section ID into section object
                        fsSource.Seek(BasePosition + SECTION_SECTIONID, SeekOrigin.Begin);
                        fsSource.Read(section.SectionID, 0, 2);

                        //Read section CHECKSUM - IMPORTANT
                        fsSource.Seek(BasePosition + SECTION_CHECKSUM, SeekOrigin.Begin);
                        fsSource.Read(section.Checksum, 0, 2);

                        //Read signature into section object
                        fsSource.Seek(BasePosition + SECTION_SIGNATURE, SeekOrigin.Begin);
                        fsSource.Read(section.Signature, 0, 4);

                        //Read save index into section object
                        fsSource.Seek(BasePosition + SECTION_SAVEINDEX, SeekOrigin.Begin);
                        fsSource.Read(section.SaveIndex, 0, 4);

                        var newChecksum = section.GetChecksumForData(section.Data.Data, Utilities.GetSectionSize(BitConverter.ToUInt16(section.SectionID)));
                        var saveChecksum = BitConverter.ToUInt16(section.Checksum);

                        if (newChecksum != saveChecksum)
                        {
                            ConsoleLog(ELogType.Warning, $"Checksum {saveChecksum} does not match needed checksum {newChecksum}.");
                        }

                        fileA.Sections[i] = section;
                        ConsoleLog(ELogType.Debug, $"Added section {BitConverter.ToUInt16(section.SectionID)}");
                    }

                    //The same, but for file B
                    for (int i = 0; i < NumSections; i++)
                    {
                        Section section = new Section();
                        var BasePosition = fsSource.Position;

                        section.SectionPosition = (int)BasePosition;

                        //Read main section data into section object
                        fsSource.Read(section.Data.Data, 0, 3968);

                        //Read section ID into section object
                        fsSource.Seek(BasePosition + SECTION_SECTIONID, SeekOrigin.Begin);
                        fsSource.Read(section.SectionID, 0, 2);

                        //Read section CHECKSUM - IMPORTANT
                        fsSource.Seek(BasePosition + SECTION_CHECKSUM, SeekOrigin.Begin);
                        fsSource.Read(section.Checksum, 0, 2);

                        //Read signature into section object
                        fsSource.Seek(BasePosition + SECTION_SIGNATURE, SeekOrigin.Begin);
                        fsSource.Read(section.Signature, 0, 4);

                        //Read save index into section object
                        fsSource.Seek(BasePosition + SECTION_SAVEINDEX, SeekOrigin.Begin);
                        fsSource.Read(section.SaveIndex, 0, 4);

                        var newChecksum = section.GetChecksumForData(section.Data.Data, Utilities.GetSectionSize(BitConverter.ToUInt16(section.SectionID)));
                        var saveChecksum = BitConverter.ToUInt16(section.Checksum);

                        if (newChecksum != saveChecksum)
                        {
                            ConsoleLog(ELogType.Warning, $"Checksum {saveChecksum} does not match needed checksum {newChecksum}.");
                        }


                        fileB.Sections[i] = section;
                        ConsoleLog(ELogType.Debug, $"Added section {BitConverter.ToUInt16(section.SectionID)}");

                    }

                    if (BitConverter.ToUInt16(fileA.Sections[13].SaveIndex) > BitConverter.ToUInt16(fileB.Sections[13].SaveIndex))
                        usedFile = fileA;
                    else
                        usedFile = fileB;

                    OnFileSystemInitialized(usedFile);
                }
            }
            catch (Exception e)
            {
                ConsoleLog(ELogType.Error, "An Error occurred while reading save file: " + e.ToString());
            }
            
        }

        private void folderBrowserDialog1_HelpRequest(object sender, EventArgs e)
        {

        }

        private void groupBox1_Enter(object sender, EventArgs e)
        {

        }

        private void groupBox1_Enter_1(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            openFileDialog1.Filter =
                        "Save Files|*.sav" +
                        "|All Files|*.*";
            openFileDialog1.FilterIndex = 1;
            var result = openFileDialog1.ShowDialog();

            if (result == DialogResult.OK)
            {
                ConsoleLog(ELogType.Info, $"File selected: {openFileDialog1.SafeFileName}");
                InitializeFileSystem(openFileDialog1.FileName);
            }
        }

        private void label2_Click(object sender, EventArgs e)
        {

        }

        private void label3_Click(object sender, EventArgs e)
        {

        }

        private void ChecksumSections_SelectedIndexChanged(object sender, EventArgs e)
        {
            int index = ChecksumSections.SelectedIndex;
            ChecksumValue.Text = BitConverter.ToUInt16(usedFile.Sections[index].Checksum).ToString();
            ChecksumValue.Visible = true;
            ChecksumLabel.Visible = true;
            button2.Visible = true;
            button3.Visible = true;

            var location = usedFile.Sections[index].SectionPosition.ToString("X");
#if DEBUG
            sectionLocationOffset.Visible = true;
            sectionLocationOffset.Text = "0x" + location;
#endif

            var newChecksum = usedFile.Sections[index].GetChecksumForData(usedFile.Sections[index].Data.Data, Utilities.GetSectionSize(BitConverter.ToUInt16(usedFile.Sections[index].SectionID)));
            
            if (BitConverter.ToUInt16(usedFile.Sections[index].Checksum) != newChecksum)
            {
                ChecksumErrorImage.Visible = true;
            }
            else
            {
                ChecksumErrorImage.Visible = false;
            }
        }

        private void CheckChecksums_Click(object sender, EventArgs e)
        {
            //credits menu

            if (ChecksumSections.SelectedIndex != -1)
            {
                if (Utilities.GetSectionName(BitConverter.ToUInt16(usedFile.Sections[ChecksumSections.SelectedIndex].SectionID)) == "Trainer Info")
                {
                    if (ChecksumValue.Text == "61907")
                    {
                        creditsGroup.Visible = true;
                        creditsLabel.Visible = true;
                        ConsoleLog(Color.Purple, "[SECRET] Thanks for using me!");
                        return;
                    }
                }
            }
            
            bool bSuccess = true;
            foreach (var section in usedFile.Sections)
            {
                var newChecksum = section.GetChecksumForData(section.Data.Data, Utilities.GetSectionSize(BitConverter.ToUInt16(section.SectionID)));
                var checksum = BitConverter.ToUInt16(section.Checksum);
                
                if (checksum != newChecksum)
                {
                    MessageBox.Show($"Checksum for Section {BitConverter.ToUInt16(section.SectionID)} ({Utilities.GetSectionName(BitConverter.ToUInt16(section.SectionID))}) is invalid.", "Invalid Checksum", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    bSuccess = false;
                }
            }

            if (bSuccess)
            {
                MessageBox.Show("Checksums valid!", "Valid Checksums", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void ChecksumValue_TextChanged(object sender, EventArgs e)
        {
            int index = ChecksumSections.SelectedIndex;
            var newChecksum = usedFile.Sections[index].GetChecksumForData(usedFile.Sections[index].Data.Data, Utilities.GetSectionSize(BitConverter.ToUInt16(usedFile.Sections[index].SectionID)));

            UInt16 oldSum;
            if (UInt16.TryParse(ChecksumValue.Text, out oldSum))
            {
                if ( oldSum != newChecksum)
                {
                    ChecksumErrorImage.Visible = true;
                }
                else
                {
                    ChecksumErrorImage.Visible = false;
                }
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            int index = ChecksumSections.SelectedIndex;
            int location = usedFile.Sections[index].SectionPosition;
            UInt16 newChecksum;
            if (ChecksumErrorImage.Visible)
            {
                var result = MessageBox.Show("Checksum value will not be valid, are you sure you want to save?", "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

                if (result == DialogResult.No)
                    return;
            }

            if (UInt16.TryParse(ChecksumValue.Text, out newChecksum))
            {
                Utilities.WriteToSection(FileName, location, 2, BitConverter.GetBytes(newChecksum), SECTION_CHECKSUM);
                usedFile.Sections[index].Checksum = BitConverter.GetBytes(newChecksum);
                ConsoleLog(ELogType.Info, "Wrote Checksum successfully!");
            }
            else
            {
                ConsoleLog(ELogType.Error, "Invalid Checksum Value.");
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            var usedSection = usedFile.Sections[ChecksumSections.SelectedIndex];
            var newChecksum = usedSection.GetChecksumForData(usedSection.Data.Data, Utilities.GetSectionSize(BitConverter.ToUInt16(usedSection.SectionID)));
            ChecksumValue.Text = newChecksum.ToString();
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private long GetPkmnOffset(int PokemonIndex)
        {
            switch (PokemonIndex)
            {
                case 0:
                    return TEAMPKMN_1;
                    break;
                case 1:
                    return TEAMPKMN_2;
                    break;
                case 2:
                    return TEAMPKMN_3;
                    break;
                case 3:
                    return TEAMPKMN_4;
                    break;
                case 4:
                    return TEAMPKMN_5;
                    break;
                case 5:
                    return TEAMPKMN_6;
                    break;
            }

            return TEAMPKMN_1;
        }

        private byte[] GetRawPokemonData(int Index)
        {
            byte[] retVal = new byte[100]; 
            var section = usedFile.GetByID(1);
            using (MemoryStream memStream = new MemoryStream(section.Data.Data))
            {
                ConsoleLog(ELogType.Debug, (0x0038 + 100 * Index).ToString());
                memStream.Seek(0x0038 + (100 * Index), SeekOrigin.Begin);
                memStream.Read(retVal, 0, 100);
            }

            return retVal;
        }

        private void WriteToPkmnEasy(int size, byte[] data, long propertyIndex)
        {
            Utilities.WriteToSection(FileName, usedFile.GetByID(1).SectionPosition, size, data, GetPkmnOffset(PokemonCombo.SelectedIndex) + propertyIndex);
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                byte[] nameToWrite = Utilities.EncryptString(PlayerNameBox.Text, 7);
                Utilities.WriteToSection(FileName, usedFile.GetByID(0).SectionPosition, 7, nameToWrite, 0x0000);
                byte genderValue;
                Utilities.ParseHex("0x" + GenderBox.SelectedIndex.ToString(), out genderValue);
                Utilities.WriteToSection(FileName, usedFile.GetByID(0).SectionPosition, 1, new byte[1] { genderValue }, 0x0008);

                UInt32 boxValue;
                if (UInt32.TryParse(MoneyBox.Text, out boxValue))
                {
                    boxValue = BitConverter.ToUInt32(TrainerInfo.SecurityKey) ^ boxValue;
                    Utilities.WriteToSection(FileName, usedFile.GetByID(1).SectionPosition, 4, BitConverter.GetBytes(boxValue), 0x0290);
                }
                else
                {
                    ConsoleLog(ELogType.Error, "Invalid money value.");
                }

                PlayTime newPlaytime = new PlayTime();
                PlayTime oldPlaytime = Utilities.GetPlayTime(TrainerInfo.TimePlayed);

                UInt16.TryParse(HoursBox.Text, out newPlaytime.Hours);
                sbyte.TryParse(MinutesBox.Text, out newPlaytime.Minutes);
                sbyte.TryParse(SecondsBox.Text, out newPlaytime.Seconds);
                newPlaytime.Frames = oldPlaytime.Frames;

                byte[] toWrite = Utilities.EncryptPlaytime(newPlaytime);
                Utilities.WriteToSection(FileName, usedFile.GetByID(0).SectionPosition, 5, toWrite, 0x000E);
                ConsoleLog(ELogType.Info, "Wrote playtime successfully!");
                Utilities.WriteToSection(FileName, usedFile.GetByID(1).SectionPosition, 10, Utilities.EncryptString(PokemonNicknameBox.Text, 10), GetPkmnOffset(PokemonCombo.SelectedIndex) + 8);
                Utilities.WriteToSection(FileName, usedFile.GetByID(1).SectionPosition, 7, Utilities.EncryptString(PokemonTrainerNameBox.Text, 7), GetPkmnOffset(PokemonCombo.SelectedIndex) + 20);
                Utilities.WriteToSection(FileName, usedFile.GetByID(1).SectionPosition, 1, new byte[1] { byte.Parse(LevelBox.Text) }, GetPkmnOffset(PokemonCombo.SelectedIndex) + 84);
                WriteToPkmnEasy(2, BitConverter.GetBytes(UInt16.Parse(CurHpBox.Text)), 86);
                WriteToPkmnEasy(2, BitConverter.GetBytes(UInt16.Parse(MaxHpBox.Text)), 88);
                int originalLocation = PokemonCombo.SelectedIndex;
                InitializeFileSystem(FileName);


                Utilities.WriteToSection(FileName, usedFile.GetByID(1).SectionPosition, 2, BitConverter.GetBytes(Utilities.Add16(GetRawPokemonData(originalLocation))), GetPkmnOffset(originalLocation) + 28);

            }
            catch (Exception Exception)
            {
                ConsoleLog(ELogType.Error, "An error has occurred: " + Exception.ToString());
            }
        }


        private void button4_Click(object sender, EventArgs e)
        {
            try
            {
                byte[] nameToWrite = Utilities.EncryptString(PlayerNameBox.Text, 7);
                Utilities.WriteToSection(openFileDialog1.FileName, usedFile.GetByID(0).SectionPosition, 7, nameToWrite, 0x0000);
                byte genderValue;
                Utilities.ParseHex("0x" + GenderBox.SelectedIndex.ToString(), out genderValue);
                Utilities.WriteToSection(openFileDialog1.FileName, usedFile.GetByID(0).SectionPosition, 1, new byte[1] { genderValue }, 0x0008);

                UInt32 boxValue;
                if (UInt32.TryParse(MoneyBox.Text, out boxValue))
                {
                    boxValue = BitConverter.ToUInt32(TrainerInfo.SecurityKey) ^ boxValue;
                    Utilities.WriteToSection(openFileDialog1.FileName, usedFile.GetByID(1).SectionPosition, 4, BitConverter.GetBytes(boxValue), 0x0290);
                }
                else
                {
                    ConsoleLog(ELogType.Error, "Invalid money value.");
                }

                ConsoleLog(ELogType.Info, "Saved trainer info!");
                InitializeFileSystem(openFileDialog1.FileName);


            }
            catch (Exception exception)
            {
                ConsoleLog(ELogType.Error, "An error occured: " + exception.ToString());
            }
        }

        private void button5_Click(object sender, EventArgs e)
        {
            try
            {
                PlayTime newPlaytime = new PlayTime();
                PlayTime oldPlaytime = Utilities.GetPlayTime(TrainerInfo.TimePlayed);

                UInt16.TryParse(HoursBox.Text, out newPlaytime.Hours);
                sbyte.TryParse(MinutesBox.Text, out newPlaytime.Minutes);
                sbyte.TryParse(SecondsBox.Text, out newPlaytime.Seconds);
                newPlaytime.Frames = oldPlaytime.Frames;

                byte[] toWrite = Utilities.EncryptPlaytime(newPlaytime);
                Utilities.WriteToSection(openFileDialog1.FileName, usedFile.GetByID(0).SectionPosition, 5, toWrite, 0x000E);
                ConsoleLog(ELogType.Info, "Wrote playtime successfully!");
                InitializeFileSystem(openFileDialog1.FileName);
            }
            catch (Exception Exception)
            {
                ConsoleLog(ELogType.Error, "An error has occurred: " + Exception.ToString());
            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (usedFile.Sections.Length > 0)
            {
                if (!Utilities.AreAllChecksumsValid(usedFile))
                {
                    if (MessageBox.Show("Are you sure you want to exit? 1 or more checksums are invalid.", "Warning", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.No)
                    {
                        e.Cancel = true;
                    }
                }
            }
        }

        private void PokemonCombo_SelectedIndexChanged(object sender, EventArgs e)
        {

            trainerGroup.Visible = true;
            pokemonGroup.Visible = true;
            statusGroup.Visible = true;
            TIDLabel.Visible = PokemonTIDBox.Visible = true;
            SIDLabel.Visible = PokemonSIDBox.Visible = true;
            PokemonTrainerNameBox.Visible = TrainerNameLabel.Visible = true;
            NicknameLabel.Visible = PokemonNicknameBox.Visible = true;
            LevelLabel.Visible = LevelBox.Visible = true;
            HPLabel.Visible = CurHpBox.Visible = MaxHpBox.Visible = true;
            sleepChk.Visible = true;
            poisonChk.Visible = paralzChk.Visible = freezeChk.Visible = burnChk.Visible = badpoisonChk.Visible = true;

            PKMFile pokemon = Pokemons[PokemonCombo.SelectedIndex];
            var tid = GetTrainerID(pokemon.OTID);
            PokemonTIDBox.Text = tid.TID.ToString();
            PokemonSIDBox.Text = tid.SID.ToString();
            PokemonTrainerNameBox.Text = Utilities.DecryptMessage(pokemon.OTName);

            PokemonNicknameBox.Text = Utilities.DecryptMessage(pokemon.Nickname);
            LevelBox.Text = pokemon.Level[0].ToString();
            CurHpBox.Text = BitConverter.ToUInt16(pokemon.CurrentHP).ToString();
            MaxHpBox.Text = BitConverter.ToUInt16(pokemon.TotalHP).ToString();

            

            UInt32 statusCondition = BitConverter.ToUInt32(pokemon.StatusCondition);

            poisonChk.Checked = Utilities.ReadNBit(statusCondition, 3);
            var sleepTime = (statusCondition & (1 << 0 - 1)) + (statusCondition & (1 << 1 - 1)) + (statusCondition & (1 << 2 - 1));

            sleepChk.Checked = sleepTurnsBox.Visible = sleepTurnsLabel.Visible = sleepTime > 0;
            sleepTurnsBox.Text = sleepTime.ToString();

            burnChk.Checked = Utilities.ReadNBit(statusCondition, 4);
            freezeChk.Checked = Utilities.ReadNBit(statusCondition, 5);
            paralzChk.Checked = Utilities.ReadNBit(statusCondition, 6);
            badpoisonChk.Checked = Utilities.ReadNBit(statusCondition, 7);
        }

        private void label21_Click(object sender, EventArgs e)
        {

        }

        private void checkBox5_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void toolStripLabel1_Click(object sender, EventArgs e)
        {

        }

        //save 1
        private void toolStripMenuItem2_Click(object sender, EventArgs e)
        {
            try
            {
                var path = toolStripMenuItem2.Text.Split("1. ")[1];
                if (path == " " || path == "")
                    return;
                InitializeFileSystem(path);
            }
            catch (Exception exce)
            {
                Console.WriteLine(exce);
            }
        }

        //save 2
        private void toolStripMenuItem3_Click(object sender, EventArgs e)
        {
            try
            {
                var path = toolStripMenuItem3.Text.Split("2. ")[1];
                if (path == " " || path == "")
                    return;
                InitializeFileSystem(path);
            }
            catch (Exception exce)
            {
                Console.WriteLine(exce);
            }
        }

        //save 3
        private void toolStripMenuItem4_Click(object sender, EventArgs e)
        {
            try
            {
                var path = toolStripMenuItem4.Text.Split("3. ")[1];
                if (path == " " || path == "")
                    return;
                InitializeFileSystem(path);
            }
            catch (Exception exce)
            {
                Console.WriteLine(exce);
            }
        }

        //save 4
        private void toolStripMenuItem5_Click(object sender, EventArgs e)
        {
            try
            {
                var path = toolStripMenuItem5.Text.Split("4. ")[1];
                if (path == " " || path == "")
                    return;
                InitializeFileSystem(path);
            }
            catch (Exception exce)
            {
                Console.WriteLine(exce);
            }
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            button1_Click(sender, e);
        }

        private void richTextBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void clearRecentSavesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var settings = Utilities.GetSettings();
            for (int i = 0; i < 4; i++)
            {
                settings.Write($"File{i + 1}", "", "RecentFiles");

                switch (i)
                {
                    case 0:
                        toolStripMenuItem2.Text = "1. ";
                        break;
                    case 1:
                        toolStripMenuItem3.Text = "2. ";
                        break;
                    case 2:
                        toolStripMenuItem4.Text = "3. ";
                        break;
                    case 3:
                        toolStripMenuItem5.Text = "4. ";
                        break;
                }
            }
        }

       
    }

    public static class RichTextBoxExtensions
    {
        //Note: USE THIS FUNCTION, DO NOT USE box.Text +=!!!!!!
        public static void AppendText(this RichTextBox box, string text, Color color)
        {
            box.SelectionStart = box.TextLength;
            box.SelectionLength = 0;

            box.SelectionColor = color;
            box.AppendText(text);
            box.SelectionColor = box.ForeColor;
        }
    }
}