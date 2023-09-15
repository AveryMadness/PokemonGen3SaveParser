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
            Section trainerSection = usedFile.GetByID(0);
            Section itemsSection = usedFile.GetByID(1);

            using (MemoryStream memoryStream = new MemoryStream(trainerSection.Data.Data))
            using (BinaryReader binaryReader = new BinaryReader(memoryStream))
            {
                memoryStream.Read(TrainerInfo.playerName, 0, 7);
                memoryStream.Seek(0x0008, SeekOrigin.Begin);
                memoryStream.Read(TrainerInfo.playerGender, 0, 1);
                memoryStream.Seek(0x000A, SeekOrigin.Begin);
                memoryStream.Read(TrainerInfo.TrainerID, 0, 4);
                memoryStream.Seek(0x000E, SeekOrigin.Begin);
                memoryStream.Read(TrainerInfo.TimePlayed, 0, 5);
                memoryStream.Seek(0x0013, SeekOrigin.Begin);
                memoryStream.Read(TrainerInfo.Options, 0, 3);
                memoryStream.Seek(0x0F20, SeekOrigin.Begin);
                memoryStream.Read(TrainerInfo.SecurityKey, 0, 4);
            }

            RawPKMData[] pokemonData = new RawPKMData[6];

            using (MemoryStream memoryStream = new MemoryStream(itemsSection.Data.Data))
            using (BinaryReader binaryReader = new BinaryReader(memoryStream))
            {
                memoryStream.Seek(0x0034, SeekOrigin.Begin);
                memoryStream.Read(teamAndItems.TeamSize, 0, 4);
                
                memoryStream.Seek(0x0038, SeekOrigin.Begin);
                
                RawPKMData newData = new RawPKMData();
                memoryStream.Read(newData.Data, 0, 100);
                pokemonData[0] = newData;

                newData = new RawPKMData();
                memoryStream.Seek(0x9C, SeekOrigin.Begin);
                memoryStream.Read(newData.Data, 0, 100);
                pokemonData[1] = newData;

                newData = new RawPKMData();
                memoryStream.Seek(0x100, SeekOrigin.Begin);
                memoryStream.Read(newData.Data, 0, 100);
                pokemonData[2] = newData;

                newData = new RawPKMData();
                memoryStream.Seek(0x164, SeekOrigin.Begin);
                memoryStream.Read(newData.Data, 0, 100);
                pokemonData[3] = newData;

                newData = new RawPKMData();
                memoryStream.Seek(0x1C8, SeekOrigin.Begin);
                memoryStream.Read(newData.Data, 0, 100);
                pokemonData[4] = newData;

                newData = new RawPKMData();
                memoryStream.Seek(0x22C, SeekOrigin.Begin);
                memoryStream.Read(newData.Data, 0, 100);
                pokemonData[5] = newData;

                memoryStream.Seek(0x0290, SeekOrigin.Begin);
                memoryStream.Read(teamAndItems.Money, 0, 4);
            }

            foreach (RawPKMData firstPokemon in pokemonData)
            {

                using (MemoryStream memoryStream = new MemoryStream(firstPokemon.Data))
                using (BinaryReader binaryReader = new BinaryReader(memoryStream))
                {
                    PKMFile Pokemon = new PKMFile();

                    memoryStream.Read(Pokemon.PersonalityValue, 0, 4);
                    memoryStream.Seek(0x4, SeekOrigin.Begin);
                    memoryStream.Read(Pokemon.OTID, 0, 4);
                    memoryStream.Seek(0x8, SeekOrigin.Begin);
                    memoryStream.Read(Pokemon.Nickname, 0, 10);
                    memoryStream.Seek(0x14, SeekOrigin.Begin);
                    memoryStream.Read(Pokemon.OTName, 0, 7);
                    memoryStream.Seek(0x20, SeekOrigin.Begin);
                    memoryStream.Read(Pokemon.Data, 0, 48);
                    memoryStream.Seek(0x50, SeekOrigin.Begin);
                    memoryStream.Read(Pokemon.StatusCondition, 0, 4);
                    memoryStream.Seek(0x54, SeekOrigin.Begin);
                    memoryStream.Read(Pokemon.Level, 0, 1);
                    memoryStream.Seek(0x56, SeekOrigin.Begin);
                    memoryStream.Read(Pokemon.CurrentHP, 0, 2);
                    memoryStream.Seek(0x58, SeekOrigin.Begin);
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
            SetupTrainerInfo();
        }

        private void InitializeFileSystem(string fileName)
        {
            Array.Clear(fileA.Sections);
            Array.Clear(fileB.Sections);
            ConsoleLog(ELogType.Info, $"Initializing FileSystem for file {openFileDialog1.SafeFileName}");
            try
            {
                //Initialize FileStream with save file as source, Open file and Read contents
                using (FileStream fsSource = new FileStream(fileName, FileMode.Open, FileAccess.Read))
                using (BinaryReader binaryReader = new BinaryReader(fsSource)) //Initialize BinaryReader with fsSource as Stream
                {
                    ConsoleLog(ELogType.Info, "Initialized FileSystem successfully.");
                    //Set SaveFile label to file name
                    label1.Text = "" + openFileDialog1.SafeFileName;
                    label1.Visible = true;

                    for (int i = 0; i < NumSections; i++)
                    {
                        Section section = new Section();
                        var BasePosition = fsSource.Position;

                        section.SectionPosition = (int)BasePosition;


                        //Read main section data into section object
                        fsSource.Read(section.Data.Data, 0, 3968);

                        //Read section ID into section object
                        fsSource.Seek(BasePosition + 0x0FF4, SeekOrigin.Begin);
                        fsSource.Read(section.SectionID, 0, 2);

                        //Read section CHECKSUM - IMPORTANT
                        fsSource.Seek(BasePosition + 0x0FF6, SeekOrigin.Begin);
                        fsSource.Read(section.Checksum, 0, 2);

                        //Read signature into section object
                        fsSource.Seek(BasePosition + 0x0FF8, SeekOrigin.Begin);
                        fsSource.Read(section.Signature, 0, 4);

                        //Read save index into section object
                        fsSource.Seek(BasePosition + 0x0FFC, SeekOrigin.Begin);
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
                        fsSource.Seek(BasePosition + 0x0FF4, SeekOrigin.Begin);
                        fsSource.Read(section.SectionID, 0, 2);

                        //Read section CHECKSUM - IMPORTANT
                        fsSource.Seek(BasePosition + 0x0FF6, SeekOrigin.Begin);
                        fsSource.Read(section.Checksum, 0, 2);

                        //Read signature into section object
                        fsSource.Seek(BasePosition + 0x0FF8, SeekOrigin.Begin);
                        fsSource.Read(section.Signature, 0, 4);

                        //Read save index into section object
                        fsSource.Seek(BasePosition + 0x0FFC, SeekOrigin.Begin);
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
                Utilities.WriteToSection(openFileDialog1.FileName, location, 2, BitConverter.GetBytes(newChecksum), 0x0FF6);
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
            PKMFile pokemon = Pokemons[PokemonCombo.SelectedIndex];
            var tid = GetTrainerID(pokemon.OTID);
            PokemonTIDBox.Text = tid.TID.ToString();
            PokemonSIDBox.Text = tid.SID.ToString();
            PokemonTrainerNameBox.Text = Utilities.DecryptMessage(pokemon.OTName);

            PokemonNicknameBox.Text = Utilities.DecryptMessage(pokemon.Nickname);
            ConsoleLog(ELogType.Debug, $"Char: {Utilities.GetDecryptedCharacter(pokemon.Nickname[0])}");
            ConsoleLog(ELogType.Debug, $"Char: {Utilities.GetDecryptedCharacter(pokemon.Nickname[1])}");
            ConsoleLog(ELogType.Debug, $"Char: {Utilities.GetDecryptedCharacter(pokemon.Nickname[2])}");
            ConsoleLog(ELogType.Debug, $"Char: {Utilities.GetDecryptedCharacter(pokemon.Nickname[3])}");
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