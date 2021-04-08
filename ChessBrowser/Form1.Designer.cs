using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace ChessBrowser
{
  partial class Form1
  {
    /// <summary>
    /// Required designer variable.
    /// </summary>
    private System.ComponentModel.IContainer components = null;

    /// <summary>
    /// Keep track of which radio button is pressed
    /// </summary>
    private RadioButton winnerButton = null;

    /// <summary>
    /// This function handles the "Upload PGN" button.
    /// Given a filename, parses the PGN file, and uploads
    /// each chess game to the user's database.
    /// </summary>
    /// <param name="PGNfilename">The path to the PGN file</param>
    private void UploadGamesToDatabase(string PGNfilename)
    {
      // This will build a connection string to your user's database on atr,
      // assuimg you've typed a user and password in the GUI
      string connection = GetConnectionString();

      // TODO: Load and parse the PGN file
      //       We recommend creating separate libraries to represent chess data and load the file
      
      List<ChessTools.ChessGame> data = ChessTools.PGNReader.Reader(PGNfilename);
      // Use this to tell the GUI's progress bar how many total work steps there are
      // For example, one iteration of your main upload loop could be one work step
      SetNumWorkItems(data.Count);


      using (MySqlConnection conn = new MySqlConnection(connection))
      {
        try
        {
                    // Open a connection
                    conn.Open();

                    //  create command for each individual needed command request. Event, Game, Player(black and white)

                    string eName ="";
                    string site = "";
                    string date = "";
                    MySqlCommand eventcmd = conn.CreateCommand();
                    eventcmd.CommandText = "insert ignore into Events(Name, Site, Date) " +
                        "values(@eName,@site,@date);";
                    eventcmd.Parameters.AddWithValue("@eName", eName);
                    eventcmd.Parameters.AddWithValue("@site", site);
                    eventcmd.Parameters.AddWithValue("@date", date);

                    eventcmd.Prepare();


                    //#1
                    //use for inserting players to also check elo values
                    //insert into Players(Name, Elo) values("So, Wesley", 2880) ON DUPLICATE KEY UPDATE Elo = If(2880 > Elo,true value,false value) 
                    
                    //bplayer
                    string bName = "";
                    int elo = 0;
                    MySqlCommand bplayercmd = conn.CreateCommand();
                    bplayercmd.CommandText = "insert into Players(Name,Elo) " +
                        "values(@bplayer,@bElo) " + 
                        "on duplicate key update Elo = If(@bElo > Elo, @bElo, Elo);";
                    bplayercmd.Parameters.AddWithValue("@bplayer", bName);
                    bplayercmd.Parameters.AddWithValue("@bElo", elo);

                    bplayercmd.Prepare();

                    //wplayer
                    string wName = "";
                    int welo = 0;
                    MySqlCommand wplayercmd = conn.CreateCommand();
                    wplayercmd.CommandText = "insert into Players(Name,Elo) " +
                        "values(@wplayer,@wElo) " +
                        "on duplicate key update Elo = If(@wElo > Elo, @wElo, Elo);";
                    wplayercmd.Parameters.AddWithValue("@wplayer", wName);
                    wplayercmd.Parameters.AddWithValue("@wElo", welo);

                    wplayercmd.Prepare();

                    //game
                    string round = "";
                    string result = "";
                    string moves = "";
                    int bpID = 0;
                    int wpID = 0;
                    int eID = 0;
                    MySqlCommand gamecmd = conn.CreateCommand();
                    gamecmd.CommandText = "insert into Games(Round, Result, Moves, BlackPlayer, WhitePlayer, eID) " +
                        "values(@round,@result,@moves,@bpID,@wpID,@eID);";
                    gamecmd.Parameters.AddWithValue("@round", round);
                    gamecmd.Parameters.AddWithValue("@result", result);
                    gamecmd.Parameters.AddWithValue("@moves", moves);
                    gamecmd.Parameters.AddWithValue("@bpID", bpID);
                    gamecmd.Parameters.AddWithValue("@wpID", wpID);
                    gamecmd.Parameters.AddWithValue("@eID", eID);

                    gamecmd.Prepare();

                    //find pID per user Name
                    string name = "";
                    MySqlCommand findpID = conn.CreateCommand();
                    findpID.CommandText = "select pID from Players where Name = @Name;";
                    findpID.Parameters.AddWithValue("@Name", name);

                    findpID.Prepare();

                    //find eID per Event game
                    string ename = "";
                    string edate = "";
                    string esite = "";
                    MySqlCommand findeID = conn.CreateCommand();
                    findeID.CommandText = "select eID from Events where Name = @eName and Date = @edate and Site = @Site;";
                    findeID.Parameters.AddWithValue("@eName", ename);
                    findeID.Parameters.AddWithValue("@edate", edate);
                    findeID.Parameters.AddWithValue("@Site", esite);

                    findeID.Prepare();

                    // TODO: iterate through your data and generate appropriate insert commands
                    foreach (var item in data)
                    {

                        //insert ignore event(Name,Site,Date)
                        eventcmd.Parameters["@eName"].Value = item.EName;
                        eventcmd.Parameters["@site"].Value = item.Site;
                        eventcmd.Parameters["@date"].Value = item.EventDate;
                        eventcmd.ExecuteNonQuery();

                        //insert both black and white player, use #1
                        bplayercmd.Parameters["@bplayer"].Value = item.BPlayer;
                        bplayercmd.Parameters["@bElo"].Value = item.BElo;
                        bplayercmd.ExecuteNonQuery();

                        wplayercmd.Parameters["@wplayer"].Value = item.WPlayer;
                        wplayercmd.Parameters["@wElo"].Value = item.WElo;
                        wplayercmd.ExecuteNonQuery();

                        //select black player pID from table compared to name
                        int bID = 0;
                        findpID.Parameters["@Name"].Value = item.BPlayer;
                        using (MySqlDataReader reader = findpID.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                bID = reader.GetInt32(0);
                            }
                        }

                        //select white player pID from table compared to name
                        int wID = 0;
                        findpID.Parameters["@Name"].Value = item.WPlayer;
                        using (MySqlDataReader reader = findpID.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                wID = reader.GetInt32(0);
                            }
                        }

                        int feID = 0;
                        findeID.Parameters["@eName"].Value = item.EName;
                        findeID.Parameters["@edate"].Value = item.EventDate;
                        findeID.Parameters["@Site"].Value = item.Site;
                        using (MySqlDataReader reader = findeID.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                feID = reader.GetInt32(0);
                            }
                        }

                        //insert game
                        gamecmd.Parameters["@round"].Value = item.Round;
                        gamecmd.Parameters["@result"].Value = item.Result;
                        gamecmd.Parameters["@moves"].Value = item.Moves;
                        gamecmd.Parameters["@bpID"].Value = bID;
                        gamecmd.Parameters["@wpID"].Value = wID;
                        gamecmd.Parameters["@eID"].Value = feID;
                        gamecmd.ExecuteNonQuery();

                        // Use this to tell the GUI that one work step has completed:
                        WorkStepCompleted();
                    }


        }
        catch (Exception e)
        {
          Console.WriteLine(e.Message);
        }
      }

    }

    
    /// <summary>
    /// Queries the database for games that match all the given filters.
    /// The filters are taken from the various controls in the GUI.
    /// </summary>
    /// <param name="white">The white player, or "" if none</param>
    /// <param name="black">The black player, or "" if none</param>
    /// <param name="opening">The first move, e.g. "e4", or "" if none</param>
    /// <param name="winner">The winner as "White", "Black", "Draw", or "" if none</param>
    /// <param name="useDate">True if the filter includes a date range, False otherwise</param>
    /// <param name="start">The start of the date range</param>
    /// <param name="end">The end of the date range</param>
    /// <param name="showMoves">True if the returned data should include the PGN moves</param>
    /// <returns>A string separated by windows line endings ("\r\n") containing the filtered games</returns>
    private string PerformQuery(string white, string black, string opening, 
      string winner, bool useDate, DateTime start, DateTime end, bool showMoves)
    {
      // This will build a connection string to your user's database on atr,
      // assuimg you've typed a user and password in the GUI
      string connection = GetConnectionString();
      
      // Build up this string containing the results from your query
      string parsedResult = "";

      // Use this to count the number of rows returned by your query
      // (see below return statement)
      int numRows = 0;

      using (MySqlConnection conn = new MySqlConnection(connection))
      {
        try
        {
          // Open a connection
          conn.Open();

                    MySqlCommand cmd = conn.CreateCommand();

                    List<String> variables = new List<String>();
                    List<String> prep = new List<String>();

                    // TODO: Generate and execute an SQL command,
                    //       then parse the results into an appropriate string
                    //       and return it.
                    //       Remember that the returned string must use \r\n newlines
                    
                    //starting statement of what we want from database
                    string statement = "";
                    if (showMoves)
                    {
                        statement = "select Events.Name as EventName, Site, Date, w.Name as white, b.Name as black, Result, Moves from Events natural join Games, Players as w, Players as b where WhitePlayer = w.pID and BlackPlayer = b.pID";
                    }
                    else
                    {
                        statement = "select Events.Name as EventName, Site, Date, w.Name as white, b.Name as black, Result from Events natural join Games, Players as w, Players as b where WhitePlayer = w.pID and BlackPlayer = b.pID";
                        
                    }
                    
                    // if white player needed add condition
                    if (!white.Equals(""))
                    {
                        statement += " and w.Name = @white";
                        variables.Add(white);
                        prep.Add("@white");
                    }
                    
                    //if black player needed add condition
                    if (!black.Equals(""))
                    {
                        statement += " and b.Name = @black";
                        variables.Add(black);
                        prep.Add("@black");
                    }
                    
                    //check first move by substring(moves,1, length of opening)
                    if (!opening.Equals(""))
                    {
                        string length = opening.Length.ToString();
                        statement += " and substring(Moves,1," + length +") = @opening";
                        variables.Add(opening);
                        prep.Add("@opening");
                    }
                    
                    //results condition
                    if ( winner.Equals("White") )
                    {
                        statement += " and Result = \"W\"";
                    }else if (winner.Equals("Black"))
                    {
                        statement += " and Result = \"B\"";
                    }
                    else if (winner.Equals("Draw"))
                    {
                        statement += " and Result = \"D\"";
                    }

                    //check date using between
                    if (useDate)
                    {
                        statement += " and Date Between @start And @end";
                        cmd.Parameters.AddWithValue("@start", start);
                        cmd.Parameters.AddWithValue("@end", end);
                    }

                    statement += ";";
                    cmd.CommandText = statement;

                    //add perams with addwith value all that were captured
                    int index = 0;
                    foreach(string item in variables)
                    {
                        cmd.Parameters.AddWithValue(prep[index], variables[index]);
                        index++;
                    }
                 
                    

                    
                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            parsedResult += "Event: " + reader["EventName"] + "\r\n";
                            parsedResult += "Site: " + reader["Site"] + "\r\n";
                            
                            //catch exception to input date to 0000-00-00
                            try { parsedResult += "Date: " + reader["Date"] + "\r\n"; }
                            catch(Exception e) { parsedResult += "Date: 0000-00-00" + "\r\n"; }
                            
                            parsedResult += "White: " + reader["white"] + "\r\n";
                            parsedResult += "Black: " + reader["black"] + "\r\n";
                            parsedResult += "Result: " + reader["Result"] + "\r\n";
                            
                            //only add moves if needed
                            if (showMoves) { 
                                parsedResult += "Moves: \r\n" + reader["Moves"] + "\r\n"; 
                            }

                            parsedResult += "\r\n";
                            numRows++;
                        }
                    }
                    
                }
        catch (Exception e)
        {
          Console.WriteLine(e.Message);
        }
      }

      return numRows + " results\r\n\r\n" + parsedResult;
    }


    /// <summary>
    /// Informs the progress bar that one step of work has been completed.
    /// Use SetNumWorkItems first
    /// </summary>
    private void WorkStepCompleted()
    {
      backgroundWorker1.ReportProgress(0);
    }

    /// <summary>
    /// Informs the progress bar how many steps of work there are.
    /// </summary>
    /// <param name="x">The number of work steps</param>
    private void SetNumWorkItems(int x)
    {
      this.Invoke(new MethodInvoker(() =>
        {
          uploadProgress.Maximum = x;
          uploadProgress.Step = 1;
        }));      
    }

    /// <summary>
    /// Reads the username and password from the text fields in the GUI
    /// and puts them into an SQL connection string for the atr server.
    /// </summary>
    /// <returns></returns>
    private string GetConnectionString()
    {
      return "server=atr.eng.utah.edu;database=" + userText.Text + ";uid=" + userText.Text + ";password=" + pwdText.Text;
    }

    /***************GUI functions below this point***************/
    /*You should not need to directly use any of these functions*/

    /// <summary>
    /// Disables the two buttons on the GUI,
    /// so that only one task can happen at once
    /// </summary>
    private void DisableControls()
    {
      uploadButton.Enabled = false;
      goButton.Enabled = false;
    }

    /// <summary>
    /// Enables the two buttons on the GUI, used after a task completes.
    /// </summary>
    private void EnableControls()
    {
      uploadButton.Enabled = true;
      goButton.Enabled = true;
    }


    /// <summary>
    /// Clean up any resources being used.
    /// </summary>
    /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
    protected override void Dispose(bool disposing)
    {
      if (disposing && (components != null))
      {
        components.Dispose();
      }
      base.Dispose(disposing);
    }

    #region Windows Form Designer generated code

    /// <summary>
    /// Required method for Designer support - do not modify
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
      this.userText = new System.Windows.Forms.TextBox();
      this.pwdText = new System.Windows.Forms.TextBox();
      this.label1 = new System.Windows.Forms.Label();
      this.label2 = new System.Windows.Forms.Label();
      this.uploadButton = new System.Windows.Forms.Button();
      this.backgroundWorker1 = new System.ComponentModel.BackgroundWorker();
      this.uploadProgress = new System.Windows.Forms.ProgressBar();
      this.label3 = new System.Windows.Forms.Label();
      this.label4 = new System.Windows.Forms.Label();
      this.whitePlayerText = new System.Windows.Forms.TextBox();
      this.blackPlayerText = new System.Windows.Forms.TextBox();
      this.startDate = new System.Windows.Forms.DateTimePicker();
      this.label6 = new System.Windows.Forms.Label();
      this.endDate = new System.Windows.Forms.DateTimePicker();
      this.dateCheckBox = new System.Windows.Forms.CheckBox();
      this.whiteWin = new System.Windows.Forms.RadioButton();
      this.label5 = new System.Windows.Forms.Label();
      this.blackWin = new System.Windows.Forms.RadioButton();
      this.drawWin = new System.Windows.Forms.RadioButton();
      this.label7 = new System.Windows.Forms.Label();
      this.label8 = new System.Windows.Forms.Label();
      this.openingMoveText = new System.Windows.Forms.TextBox();
      this.resultText = new System.Windows.Forms.TextBox();
      this.showMovesCheckBox = new System.Windows.Forms.CheckBox();
      this.goButton = new System.Windows.Forms.Button();
      this.anyRadioButton = new System.Windows.Forms.RadioButton();
      this.SuspendLayout();
      // 
      // userText
      // 
      this.userText.Location = new System.Drawing.Point(62, 12);
      this.userText.Name = "userText";
      this.userText.Size = new System.Drawing.Size(142, 26);
      this.userText.TabIndex = 0;
      // 
      // pwdText
      // 
      this.pwdText.Location = new System.Drawing.Point(341, 12);
      this.pwdText.Name = "pwdText";
      this.pwdText.PasswordChar = '*';
      this.pwdText.Size = new System.Drawing.Size(140, 26);
      this.pwdText.TabIndex = 1;
      // 
      // label1
      // 
      this.label1.AutoSize = true;
      this.label1.Location = new System.Drawing.Point(13, 12);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(43, 20);
      this.label1.TabIndex = 2;
      this.label1.Text = "User";
      this.label1.Click += new System.EventHandler(this.label1_Click);
      // 
      // label2
      // 
      this.label2.AutoSize = true;
      this.label2.Location = new System.Drawing.Point(257, 13);
      this.label2.Name = "label2";
      this.label2.Size = new System.Drawing.Size(78, 20);
      this.label2.TabIndex = 3;
      this.label2.Text = "Password";
      // 
      // uploadButton
      // 
      this.uploadButton.Location = new System.Drawing.Point(62, 82);
      this.uploadButton.Name = "uploadButton";
      this.uploadButton.Size = new System.Drawing.Size(149, 50);
      this.uploadButton.TabIndex = 4;
      this.uploadButton.Text = "Upload PGN";
      this.uploadButton.UseVisualStyleBackColor = true;
      this.uploadButton.Click += new System.EventHandler(this.button1_Click);
      // 
      // backgroundWorker1
      // 
      this.backgroundWorker1.WorkerReportsProgress = true;
      this.backgroundWorker1.DoWork += new System.ComponentModel.DoWorkEventHandler(this.backgroundWorker1_DoWork);
      this.backgroundWorker1.ProgressChanged += new System.ComponentModel.ProgressChangedEventHandler(this.backgroundWorker1_ProgressChanged);
      this.backgroundWorker1.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(this.backgroundWorker1_RunWorkerCompleted);
      // 
      // uploadProgress
      // 
      this.uploadProgress.Location = new System.Drawing.Point(261, 82);
      this.uploadProgress.Name = "uploadProgress";
      this.uploadProgress.Size = new System.Drawing.Size(900, 50);
      this.uploadProgress.TabIndex = 5;
      // 
      // label3
      // 
      this.label3.AutoSize = true;
      this.label3.Location = new System.Drawing.Point(17, 236);
      this.label3.Name = "label3";
      this.label3.Size = new System.Drawing.Size(97, 20);
      this.label3.TabIndex = 6;
      this.label3.Text = "White Player";
      // 
      // label4
      // 
      this.label4.AutoSize = true;
      this.label4.Location = new System.Drawing.Point(290, 236);
      this.label4.Name = "label4";
      this.label4.Size = new System.Drawing.Size(95, 20);
      this.label4.TabIndex = 7;
      this.label4.Text = "Black Player";
      // 
      // whitePlayerText
      // 
      this.whitePlayerText.Location = new System.Drawing.Point(116, 236);
      this.whitePlayerText.Name = "whitePlayerText";
      this.whitePlayerText.Size = new System.Drawing.Size(145, 26);
      this.whitePlayerText.TabIndex = 8;
      // 
      // blackPlayerText
      // 
      this.blackPlayerText.Location = new System.Drawing.Point(387, 236);
      this.blackPlayerText.Name = "blackPlayerText";
      this.blackPlayerText.Size = new System.Drawing.Size(145, 26);
      this.blackPlayerText.TabIndex = 9;
      // 
      // startDate
      // 
      this.startDate.Enabled = false;
      this.startDate.Location = new System.Drawing.Point(161, 319);
      this.startDate.Name = "startDate";
      this.startDate.Size = new System.Drawing.Size(300, 26);
      this.startDate.TabIndex = 11;
      // 
      // label6
      // 
      this.label6.AutoSize = true;
      this.label6.Location = new System.Drawing.Point(468, 324);
      this.label6.Name = "label6";
      this.label6.Size = new System.Drawing.Size(64, 20);
      this.label6.TabIndex = 12;
      this.label6.Text = "through";
      // 
      // endDate
      // 
      this.endDate.Enabled = false;
      this.endDate.Location = new System.Drawing.Point(538, 320);
      this.endDate.Name = "endDate";
      this.endDate.Size = new System.Drawing.Size(300, 26);
      this.endDate.TabIndex = 13;
      // 
      // dateCheckBox
      // 
      this.dateCheckBox.AutoSize = true;
      this.dateCheckBox.Location = new System.Drawing.Point(21, 320);
      this.dateCheckBox.Name = "dateCheckBox";
      this.dateCheckBox.Size = new System.Drawing.Size(131, 24);
      this.dateCheckBox.TabIndex = 14;
      this.dateCheckBox.Text = "Filter By Date";
      this.dateCheckBox.UseVisualStyleBackColor = true;
      this.dateCheckBox.CheckedChanged += new System.EventHandler(this.dateCheckBox_CheckedChanged);
      // 
      // whiteWin
      // 
      this.whiteWin.AutoSize = true;
      this.whiteWin.Location = new System.Drawing.Point(873, 237);
      this.whiteWin.Name = "whiteWin";
      this.whiteWin.Size = new System.Drawing.Size(75, 24);
      this.whiteWin.TabIndex = 15;
      this.whiteWin.TabStop = true;
      this.whiteWin.Text = "White";
      this.whiteWin.UseVisualStyleBackColor = true;
      this.whiteWin.CheckedChanged += new System.EventHandler(this.whiteWin_CheckedChanged);
      // 
      // label5
      // 
      this.label5.AutoSize = true;
      this.label5.Location = new System.Drawing.Point(801, 239);
      this.label5.Name = "label5";
      this.label5.Size = new System.Drawing.Size(59, 20);
      this.label5.TabIndex = 16;
      this.label5.Text = "Winner";
      // 
      // blackWin
      // 
      this.blackWin.AutoSize = true;
      this.blackWin.Location = new System.Drawing.Point(954, 237);
      this.blackWin.Name = "blackWin";
      this.blackWin.Size = new System.Drawing.Size(73, 24);
      this.blackWin.TabIndex = 17;
      this.blackWin.TabStop = true;
      this.blackWin.Text = "Black";
      this.blackWin.UseVisualStyleBackColor = true;
      this.blackWin.CheckedChanged += new System.EventHandler(this.blackWin_CheckedChanged);
      // 
      // drawWin
      // 
      this.drawWin.AutoSize = true;
      this.drawWin.Location = new System.Drawing.Point(1033, 237);
      this.drawWin.Name = "drawWin";
      this.drawWin.Size = new System.Drawing.Size(71, 24);
      this.drawWin.TabIndex = 18;
      this.drawWin.TabStop = true;
      this.drawWin.Text = "Draw";
      this.drawWin.UseVisualStyleBackColor = true;
      this.drawWin.CheckedChanged += new System.EventHandler(this.drawWin_CheckedChanged);
      // 
      // label7
      // 
      this.label7.AutoSize = true;
      this.label7.Location = new System.Drawing.Point(17, 180);
      this.label7.Name = "label7";
      this.label7.Size = new System.Drawing.Size(168, 20);
      this.label7.TabIndex = 19;
      this.label7.Text = "Find games filtered by:";
      // 
      // label8
      // 
      this.label8.AutoSize = true;
      this.label8.Location = new System.Drawing.Point(559, 236);
      this.label8.Name = "label8";
      this.label8.Size = new System.Drawing.Size(111, 20);
      this.label8.TabIndex = 20;
      this.label8.Text = "Opening Move";
      // 
      // openingMoveText
      // 
      this.openingMoveText.Location = new System.Drawing.Point(677, 235);
      this.openingMoveText.Name = "openingMoveText";
      this.openingMoveText.Size = new System.Drawing.Size(100, 26);
      this.openingMoveText.TabIndex = 21;
      // 
      // resultText
      // 
      this.resultText.Location = new System.Drawing.Point(17, 446);
      this.resultText.Multiline = true;
      this.resultText.Name = "resultText";
      this.resultText.ReadOnly = true;
      this.resultText.ScrollBars = System.Windows.Forms.ScrollBars.Both;
      this.resultText.Size = new System.Drawing.Size(1144, 654);
      this.resultText.TabIndex = 22;
      // 
      // showMovesCheckBox
      // 
      this.showMovesCheckBox.AutoSize = true;
      this.showMovesCheckBox.Location = new System.Drawing.Point(17, 387);
      this.showMovesCheckBox.Name = "showMovesCheckBox";
      this.showMovesCheckBox.Size = new System.Drawing.Size(125, 24);
      this.showMovesCheckBox.TabIndex = 23;
      this.showMovesCheckBox.Text = "Show Moves";
      this.showMovesCheckBox.UseVisualStyleBackColor = true;
      // 
      // goButton
      // 
      this.goButton.BackColor = System.Drawing.Color.Silver;
      this.goButton.Location = new System.Drawing.Point(148, 377);
      this.goButton.Name = "goButton";
      this.goButton.Size = new System.Drawing.Size(133, 42);
      this.goButton.TabIndex = 24;
      this.goButton.Text = "Go!";
      this.goButton.UseVisualStyleBackColor = false;
      this.goButton.Click += new System.EventHandler(this.button1_Click_1);
      // 
      // anyRadioButton
      // 
      this.anyRadioButton.AutoSize = true;
      this.anyRadioButton.Checked = true;
      this.anyRadioButton.Location = new System.Drawing.Point(1110, 237);
      this.anyRadioButton.Name = "anyRadioButton";
      this.anyRadioButton.Size = new System.Drawing.Size(59, 24);
      this.anyRadioButton.TabIndex = 25;
      this.anyRadioButton.TabStop = true;
      this.anyRadioButton.Text = "any";
      this.anyRadioButton.UseVisualStyleBackColor = true;
      this.anyRadioButton.CheckedChanged += new System.EventHandler(this.radioButton1_CheckedChanged);
      // 
      // Form1
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ClientSize = new System.Drawing.Size(1185, 1112);
      this.Controls.Add(this.anyRadioButton);
      this.Controls.Add(this.goButton);
      this.Controls.Add(this.showMovesCheckBox);
      this.Controls.Add(this.resultText);
      this.Controls.Add(this.openingMoveText);
      this.Controls.Add(this.label8);
      this.Controls.Add(this.label7);
      this.Controls.Add(this.drawWin);
      this.Controls.Add(this.blackWin);
      this.Controls.Add(this.label5);
      this.Controls.Add(this.whiteWin);
      this.Controls.Add(this.dateCheckBox);
      this.Controls.Add(this.endDate);
      this.Controls.Add(this.label6);
      this.Controls.Add(this.startDate);
      this.Controls.Add(this.blackPlayerText);
      this.Controls.Add(this.whitePlayerText);
      this.Controls.Add(this.label4);
      this.Controls.Add(this.label3);
      this.Controls.Add(this.uploadProgress);
      this.Controls.Add(this.uploadButton);
      this.Controls.Add(this.label2);
      this.Controls.Add(this.label1);
      this.Controls.Add(this.pwdText);
      this.Controls.Add(this.userText);
      this.Name = "Form1";
      this.Text = "Form1";
      this.Load += new System.EventHandler(this.Form1_Load);
      this.ResumeLayout(false);
      this.PerformLayout();

    }

    #endregion

    private System.Windows.Forms.TextBox userText;
    private System.Windows.Forms.TextBox pwdText;
    private System.Windows.Forms.Label label1;
    private System.Windows.Forms.Label label2;
    private System.Windows.Forms.Button uploadButton;
    private System.ComponentModel.BackgroundWorker backgroundWorker1;
    private System.Windows.Forms.ProgressBar uploadProgress;
    private Label label3;
    private Label label4;
    private TextBox whitePlayerText;
    private TextBox blackPlayerText;
    private DateTimePicker startDate;
    private Label label6;
    private DateTimePicker endDate;
    private CheckBox dateCheckBox;
    private RadioButton whiteWin;
    private Label label5;
    private RadioButton blackWin;
    private RadioButton drawWin;
    private Label label7;
    private Label label8;
    private TextBox openingMoveText;
    private TextBox resultText;
    private CheckBox showMovesCheckBox;
    private Button goButton;
    private RadioButton anyRadioButton;
  }
}

