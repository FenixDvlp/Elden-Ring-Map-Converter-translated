using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FeHeTool;

public class MainForm : Form
{
	private TextBox convIn;

	private TextBox convOut;

	private TextBox convLog;

	private TextBox mergeBase;

	private TextBox mergeAdd;

	private TextBox mergeOut;

	private TextBox mergeLog;

	private TextBox mergeCollRemap;

	private CheckBox mergeLinkEnemyGroup;

	private TextBox assetMap;

	private TextBox assetInDir;

	private TextBox assetOutDir;

	private TextBox assetLog;

	private RadioButton assetAetPrecise;

	private RadioButton assetAetDeep;

	private RadioButton assetAetWhole;

	private TextBox vbsInDir;

	private TextBox vbsOutDir;

	private TextBox vbsLog;

	private Button vbsBtn;

	private TextBox mpInDir;

	private TextBox mpOutDir;

	private TextBox mpMapId;

	private TextBox mpLog;

	private Button mpBtn;

	private TextBox eventBase;

	private TextBox eventAdd;

	private TextBox eventOut;

	private TextBox eventRemap;

	private TextBox eventLog;

	private Button eventBtn;

	private TextBox cfBase;

	private TextBox cfAdd;

	private TextBox cfOut;

	private TextBox cfRemap;

	private TextBox cfLog;

	private Button cfBtn;

	private TextBox mcBaseH;

	private TextBox mcAddH;

	private TextBox mcBaseL;

	private TextBox mcAddL;

	private TextBox mcOutH;

	private TextBox mcOutL;

	private TextBox mcRemap;

	private TextBox mcLog;

	private Button mcBtn;

	private CheckBox mcDropHigh;

	private Button convBtn;

	private Button mergeBtn;

	private Button assetBtn;

	private Button manifestBtn;

	private TextBox fixIn;

	private TextBox fixOut;

	private TextBox fixLog;

	private Button fixBtn;

	private TextBox enemyIn;

	private TextBox enemyOut;

	private TextBox enemyLog;

	private Button enemyBtn;

	private TextBox rotcolMerged;

	private TextBox rotcolM13;

	private TextBox rotcolOut;

	private TextBox rotcolLog;

	private Button rotcolBtn;

	private TextBox npcMsb;

	private TextBox npcCsv;

	private TextBox npcThinkCsv;

	private TextBox npcOut;

	private TextBox npcLog;

	private Button npcBtn;

	private CheckBox npcSecondPass;

	private TextBox aaAddH;

	private TextBox aaBaseH;

	private TextBox aaMsbMerged;

	private TextBox aaMsbOrig;

	private TextBox aaOutDir;

	private TextBox aaLog;

	private Button aaBtn;

	private const string Filter = "MSB 地图文件 (*.msb.dcx)|*.msb.dcx|所有文件 (*.*)|*.*";

	private const string EmevdFilter = "事件文件 (*.emevd.dcx)|*.emevd.dcx|所有文件 (*.*)|*.*";

	private const string HkxbhdFilter = "碰撞档案头 (*.hkxbhd)|*.hkxbhd|所有文件 (*.*)|*.*";

	private const string TxtFilter = "文本文件 (*.txt)|*.txt|所有文件 (*.*)|*.*";

	public MainForm()
	{
		Text = "Elden Ring → Nightreign Map Tool";
		Width = 760;
		Height = 600;
		StartPosition = FormStartPosition.CenterScreen;
		Font = new Font("Microsoft YaHei UI", 9f);
		MinimumSize = new Size(640, 480);
		TabControl tabControl = new TabControl
		{
			Dock = DockStyle.Fill
		};
		tabControl.TabPages.Add(BuildConvertTab());
		tabControl.TabPages.Add(BuildMergeTab());
		tabControl.TabPages.Add(BuildAssetTab());
		tabControl.TabPages.Add(BuildVbsTab());
		tabControl.TabPages.Add(BuildMapPieceTab());
		tabControl.TabPages.Add(BuildEventTab());
		tabControl.TabPages.Add(BuildCommonFuncTab());
		tabControl.TabPages.Add(BuildMapCollisionTab());
		tabControl.TabPages.Add(BuildFixMapIdTab());
		tabControl.TabPages.Add(BuildFixEnemyMapIdTab());
		tabControl.TabPages.Add(BuildAutoAlignTab());
		tabControl.TabPages.Add(BuildFixRotCollisionTab());
		tabControl.TabPages.Add(BuildFixNpcParamTab());
		Controls.Add(tabControl);
		Label value = new Label
		{
			Dock = DockStyle.Bottom,
			Height = 26,
			Text = "  Note: Real models need to be imported separately from Elden Ring resources into Nightreign. This tool is responsible for correctly converting the MSB (placement/params/references).",
			ForeColor = Color.DimGray,
			TextAlign = ContentAlignment.MiddleLeft
		};
		Controls.Add(value);
	}

	private TabPage BuildConvertTab()
	{
		TabPage tabPage = new TabPage("Format conversion (Elden Ring → Nightreign)");
		TableLayoutPanel tableLayoutPanel = new TableLayoutPanel
		{
			Dock = DockStyle.Fill,
			Padding = new Padding(12),
			ColumnCount = 3,
			RowCount = 4
		};
		tableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 110f));
		tableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
		tableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 90f));
		tableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 40f));
		tableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 40f));
		tableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 44f));
		tableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));
		tableLayoutPanel.Controls.Add(new Label
		{
			Text = "Elden Ring map:",
			TextAlign = ContentAlignment.MiddleLeft,
			Dock = DockStyle.Fill
		}, 0, 0);
		convIn = new TextBox
		{
			Dock = DockStyle.Fill,
			Anchor = (AnchorStyles.Left | AnchorStyles.Right)
		};
		convIn.TextChanged += (object? s, EventArgs e) =>
		{
			AutoFillConvOut();
		};
		tableLayoutPanel.Controls.Add(convIn, 1, 0);
		Button button = new Button
		{
			Text = "Browse...",
			Dock = DockStyle.Fill
		};
		button.Click += (object? s, EventArgs e) =>
		{
			string text = PickOpen();
			if (text != null)
			{
				convIn.Text = text;
			}
		};
		tableLayoutPanel.Controls.Add(button, 2, 0);
		tableLayoutPanel.Controls.Add(new Label
		{
			Text = "Output location:",
			TextAlign = ContentAlignment.MiddleLeft,
			Dock = DockStyle.Fill
		}, 0, 1);
		convOut = new TextBox
		{
			Dock = DockStyle.Fill
		};
		tableLayoutPanel.Controls.Add(convOut, 1, 1);
		Button button2 = new Button
		{
			Text = "Browse...",
			Dock = DockStyle.Fill
		};
		button2.Click += (object? s, EventArgs e) =>
		{
			string text = PickSave(convOut.Text);
			if (text != null)
			{
				convOut.Text = text;
			}
		};
		tableLayoutPanel.Controls.Add(button2, 2, 1);
		convBtn = new Button
		{
			Text = "Start conversion",
			Dock = DockStyle.Fill,
			BackColor = Color.FromArgb(60, 120, 200),
			ForeColor = Color.White,
			FlatStyle = FlatStyle.Flat
		};
		convBtn.Click += async (object? s, EventArgs e) =>
		{
			await RunConvert();
		};
		tableLayoutPanel.Controls.Add(convBtn, 1, 2);
		convLog = MakeLog();
		tableLayoutPanel.Controls.Add(convLog, 0, 3);
		tableLayoutPanel.SetColumnSpan(convLog, 3);
		tabPage.Controls.Add(tableLayoutPanel);
		return tabPage;
	}

	private TabPage BuildFixMapIdTab()
	{
		TabPage tabPage = new TabPage("Fix collision MapID");
		TableLayoutPanel tableLayoutPanel = new TableLayoutPanel
		{
			Dock = DockStyle.Fill,
			Padding = new Padding(12),
			ColumnCount = 3,
			RowCount = 5
		};
		tableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 110f));
		tableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
		tableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 90f));
		tableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 40f));
		tableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 40f));
		tableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 54f));
		tableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 44f));
		tableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));
		tableLayoutPanel.Controls.Add(new Label
		{
			Text = "Nightreign MSB:",
			TextAlign = ContentAlignment.MiddleLeft,
			Dock = DockStyle.Fill
		}, 0, 0);
		fixIn = new TextBox
		{
			Dock = DockStyle.Fill
		};
		fixIn.TextChanged += (object? s, EventArgs e) =>
		{
			if (fixIn.Text.Length > 0 && fixOut.Text.Length == 0)
			{
				string directoryName = Path.GetDirectoryName(fixIn.Text);
				string fileName = Path.GetFileName(fixIn.Text);
				fixOut.Text = (string.IsNullOrEmpty(directoryName) ? ("fixed_" + fileName) : Path.Combine(directoryName, "fixed_" + fileName));
			}
		};
		tableLayoutPanel.Controls.Add(fixIn, 1, 0);
		Button button = new Button
		{
			Text = "Browse...",
			Dock = DockStyle.Fill
		};
		button.Click += (object? s, EventArgs e) =>
		{
			string text = PickOpen();
			if (text != null)
			{
				fixIn.Text = text;
			}
		};
		tableLayoutPanel.Controls.Add(button, 2, 0);
		tableLayoutPanel.Controls.Add(new Label
		{
			Text = "Output location:",
			TextAlign = ContentAlignment.MiddleLeft,
			Dock = DockStyle.Fill
		}, 0, 1);
		fixOut = new TextBox
		{
			Dock = DockStyle.Fill
		};
		tableLayoutPanel.Controls.Add(fixOut, 1, 1);
		Button button2 = new Button
		{
			Text = "Browse...",
			Dock = DockStyle.Fill
		};
		button2.Click += (object? s, EventArgs e) =>
		{
			string text = PickSave(fixOut.Text);
			if (text != null)
			{
				fixOut.Text = text;
			}
		};
		tableLayoutPanel.Controls.Add(button2, 2, 1);
		Label control = new Label
		{
			Text = "Changes TileData.MapID [0,0,0,0] of collision pieces in an existing Nightreign MSB to [-1,-1,-1,-1] (the tile ownership value required for Nightreign collision).\nUsed to repair maps merged by earlier versions that crash on load. The new version's format conversion/merge already handles this automatically, so there is no need to fix it manually.",
			TextAlign = ContentAlignment.MiddleLeft,
			Dock = DockStyle.Fill,
			ForeColor = Color.DimGray
		};
		tableLayoutPanel.Controls.Add(control, 0, 2);
		tableLayoutPanel.SetColumnSpan(control, 3);
		fixBtn = new Button
		{
			Text = "Fix",
			Dock = DockStyle.Fill,
			BackColor = Color.FromArgb(180, 100, 60),
			ForeColor = Color.White,
			FlatStyle = FlatStyle.Flat
		};
		fixBtn.Click += async (object? s, EventArgs e) =>
		{
			await RunFixMapID();
		};
		tableLayoutPanel.Controls.Add(fixBtn, 1, 3);
		fixLog = MakeLog();
		tableLayoutPanel.Controls.Add(fixLog, 0, 4);
		tableLayoutPanel.SetColumnSpan(fixLog, 3);
		tabPage.Controls.Add(tableLayoutPanel);
		return tabPage;
	}

	private async Task RunFixMapID()
	{
		string inP = fixIn.Text.Trim();
		string outP = fixOut.Text.Trim();
		if (string.IsNullOrEmpty(inP) || !File.Exists(inP))
		{
			MessageBox.Show("Please select a valid Nightreign MSB file.", "Notice");
			return;
		}
		if (string.IsNullOrEmpty(outP))
		{
			MessageBox.Show("Please specify the output location.", "Notice");
			return;
		}
		if (string.Equals(Path.GetFullPath(inP), Path.GetFullPath(outP), StringComparison.OrdinalIgnoreCase))
		{
			MessageBox.Show("The input and output cannot be the same file(to avoid overwriting the original file).", "Notice");
			return;
		}
		fixBtn.Enabled = false;
		fixLog.Clear();
		try
		{
			await Task.Run(() =>
			{
				MapCore.FixMsbCollisionMapID(inP, outP, (string m) =>
				{
					Log(fixLog, m);
				});
			});
			Log(fixLog, "");
			MessageBox.Show("Fix complete!\n\nOutput: " + outP + "\n\nJust put it into map/mapstudio/ to overwrite the original MSB.", "Done");
		}
		catch (Exception ex)
		{
			Log(fixLog, "");
			Log(fixLog, "!!! Error: " + ex.Message);
			if (ex.InnerException != null)
			{
				Log(fixLog, "    " + ex.InnerException.Message);
			}
			MessageBox.Show("Fix error:\n" + (ex.InnerException?.Message ?? ex.Message) + HintForError(ex), "Error");
		}
		finally
		{
			fixBtn.Enabled = true;
		}
	}

	private TabPage BuildFixEnemyMapIdTab()
	{
		TabPage tabPage = new TabPage("Fix enemy MapID");
		TableLayoutPanel tableLayoutPanel = new TableLayoutPanel
		{
			Dock = DockStyle.Fill,
			Padding = new Padding(12),
			ColumnCount = 3,
			RowCount = 5
		};
		tableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 110f));
		tableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
		tableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 90f));
		tableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 40f));
		tableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 40f));
		tableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 54f));
		tableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 44f));
		tableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));
		tableLayoutPanel.Controls.Add(new Label
		{
			Text = "Nightreign MSB:",
			TextAlign = ContentAlignment.MiddleLeft,
			Dock = DockStyle.Fill
		}, 0, 0);
		enemyIn = new TextBox
		{
			Dock = DockStyle.Fill
		};
		enemyIn.TextChanged += (object? s, EventArgs e) =>
		{
			if (enemyIn.Text.Length > 0 && enemyOut.Text.Length == 0)
			{
				string directoryName = Path.GetDirectoryName(enemyIn.Text);
				string fileName = Path.GetFileName(enemyIn.Text);
				enemyOut.Text = (string.IsNullOrEmpty(directoryName) ? ("fixed_" + fileName) : Path.Combine(directoryName, "fixed_" + fileName));
			}
		};
		tableLayoutPanel.Controls.Add(enemyIn, 1, 0);
		Button button = new Button
		{
			Text = "Browse...",
			Dock = DockStyle.Fill
		};
		button.Click += (object? s, EventArgs e) =>
		{
			string text = PickOpen();
			if (text != null)
			{
				enemyIn.Text = text;
			}
		};
		tableLayoutPanel.Controls.Add(button, 2, 0);
		tableLayoutPanel.Controls.Add(new Label
		{
			Text = "Output location:",
			TextAlign = ContentAlignment.MiddleLeft,
			Dock = DockStyle.Fill
		}, 0, 1);
		enemyOut = new TextBox
		{
			Dock = DockStyle.Fill
		};
		tableLayoutPanel.Controls.Add(enemyOut, 1, 1);
		Button button2 = new Button
		{
			Text = "Browse...",
			Dock = DockStyle.Fill
		};
		button2.Click += (object? s, EventArgs e) =>
		{
			string text = PickSave(enemyOut.Text);
			if (text != null)
			{
				enemyOut.Text = text;
			}
		};
		tableLayoutPanel.Controls.Add(button2, 2, 1);
		Label control = new Label
		{
			Text = "Sets TileData.MapID of all real enemies in an existing Nightreign MSB to [-1,-1,-1,-1] (dummy enemies DummyEnemy are left untouched).\nIn Nightreign, a monster's MapID must be -1 (= host tile) for it to spawn/display in this tile. The new version's format conversion/merge already handles this automatically, so there is no need to fix it manually.",
			TextAlign = ContentAlignment.MiddleLeft,
			Dock = DockStyle.Fill,
			ForeColor = Color.DimGray
		};
		tableLayoutPanel.Controls.Add(control, 0, 2);
		tableLayoutPanel.SetColumnSpan(control, 3);
		enemyBtn = new Button
		{
			Text = "Fix",
			Dock = DockStyle.Fill,
			BackColor = Color.FromArgb(150, 90, 170),
			ForeColor = Color.White,
			FlatStyle = FlatStyle.Flat
		};
		enemyBtn.Click += async (object? s, EventArgs e) =>
		{
			await RunFixEnemyMapID();
		};
		tableLayoutPanel.Controls.Add(enemyBtn, 1, 3);
		enemyLog = MakeLog();
		tableLayoutPanel.Controls.Add(enemyLog, 0, 4);
		tableLayoutPanel.SetColumnSpan(enemyLog, 3);
		tabPage.Controls.Add(tableLayoutPanel);
		return tabPage;
	}

	private async Task RunFixEnemyMapID()
	{
		string inP = enemyIn.Text.Trim();
		string outP = enemyOut.Text.Trim();
		if (string.IsNullOrEmpty(inP) || !File.Exists(inP))
		{
			MessageBox.Show("Please select a valid Nightreign MSB file.", "Notice");
			return;
		}
		if (string.IsNullOrEmpty(outP))
		{
			MessageBox.Show("Please specify the output location.", "Notice");
			return;
		}
		if (string.Equals(Path.GetFullPath(inP), Path.GetFullPath(outP), StringComparison.OrdinalIgnoreCase))
		{
			MessageBox.Show("The input and output cannot be the same file(to avoid overwriting the original file).", "Notice");
			return;
		}
		enemyBtn.Enabled = false;
		enemyLog.Clear();
		try
		{
			await Task.Run(() =>
			{
				MapCore.FixMsbEnemyMapID(inP, outP, (string m) =>
				{
					Log(enemyLog, m);
				});
			});
			Log(enemyLog, "");
			MessageBox.Show("Fix complete!\n\nOutput: " + outP + "\n\nJust put it into map/mapstudio/ to overwrite the original MSB.", "Done");
		}
		catch (Exception ex)
		{
			Log(enemyLog, "");
			Log(enemyLog, "!!! Error: " + ex.Message);
			if (ex.InnerException != null)
			{
				Log(enemyLog, "    " + ex.InnerException.Message);
			}
			MessageBox.Show("Fix error:\n" + (ex.InnerException?.Message ?? ex.Message) + HintForError(ex), "Error");
		}
		finally
		{
			enemyBtn.Enabled = true;
		}
	}

	private TabPage BuildFixRotCollisionTab()
	{
		TabPage tabPage = new TabPage("Fix rotated collision");
		TableLayoutPanel tableLayoutPanel = new TableLayoutPanel
		{
			Dock = DockStyle.Fill,
			Padding = new Padding(12),
			ColumnCount = 3,
			RowCount = 6
		};
		tableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 130f));
		tableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
		tableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 90f));
		tableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 40f));
		tableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 40f));
		tableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 40f));
		tableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 70f));
		tableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 44f));
		tableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));
		tableLayoutPanel.Controls.Add(new Label
		{
			Text = "Merged MSB:",
			TextAlign = ContentAlignment.MiddleLeft,
			Dock = DockStyle.Fill
		}, 0, 0);
		rotcolMerged = new TextBox
		{
			Dock = DockStyle.Fill
		};
		rotcolMerged.TextChanged += (object? s, EventArgs e) =>
		{
			if (rotcolMerged.Text.Length > 0 && rotcolOut.Text.Length == 0)
			{
				string directoryName = Path.GetDirectoryName(rotcolMerged.Text);
				string fileName = Path.GetFileName(rotcolMerged.Text);
				rotcolOut.Text = (string.IsNullOrEmpty(directoryName) ? ("fixedrot_" + fileName) : Path.Combine(directoryName, "fixedrot_" + fileName));
			}
		};
		tableLayoutPanel.Controls.Add(rotcolMerged, 1, 0);
		Button button = new Button
		{
			Text = "Browse...",
			Dock = DockStyle.Fill
		};
		button.Click += (object? s, EventArgs e) =>
		{
			string text = PickOpen();
			if (text != null)
			{
				rotcolMerged.Text = text;
			}
		};
		tableLayoutPanel.Controls.Add(button, 2, 0);
		tableLayoutPanel.Controls.Add(new Label
		{
			Text = "Elden Ring original MSB(m13):",
			TextAlign = ContentAlignment.MiddleLeft,
			Dock = DockStyle.Fill
		}, 0, 1);
		rotcolM13 = new TextBox
		{
			Dock = DockStyle.Fill
		};
		tableLayoutPanel.Controls.Add(rotcolM13, 1, 1);
		Button button2 = new Button
		{
			Text = "Browse...",
			Dock = DockStyle.Fill
		};
		button2.Click += (object? s, EventArgs e) =>
		{
			string text = PickOpen();
			if (text != null)
			{
				rotcolM13.Text = text;
			}
		};
		tableLayoutPanel.Controls.Add(button2, 2, 1);
		tableLayoutPanel.Controls.Add(new Label
		{
			Text = "Output location:",
			TextAlign = ContentAlignment.MiddleLeft,
			Dock = DockStyle.Fill
		}, 0, 2);
		rotcolOut = new TextBox
		{
			Dock = DockStyle.Fill
		};
		tableLayoutPanel.Controls.Add(rotcolOut, 1, 2);
		Button button3 = new Button
		{
			Text = "Browse...",
			Dock = DockStyle.Fill
		};
		button3.Click += (object? s, EventArgs e) =>
		{
			string text = PickSave(rotcolOut.Text);
			if (text != null)
			{
				rotcolOut.Text = text;
			}
		};
		tableLayoutPanel.Controls.Add(button3, 2, 2);
		Label control = new Label
		{
			Text = "Only fixes \"suffixed collision pieces reused with rotation\" (such as the messed-up h005000_0 and h004700_0001); collisions that are not rotated are left untouched.\nOnly modifies the MSB and does not touch the collision archives (hkxbdt is not needed). Suitable when: the collision geometry has already been shifted/merged (those without suffixes are already fine) and only the suffixed ones are messed up.\nThe offset is calculated automatically from these two MSBs. It uses the same set of corrections as \"Auto-align collision\"; this is a quick fix that only touches the MSB.",
			TextAlign = ContentAlignment.TopLeft,
			Dock = DockStyle.Fill,
			ForeColor = Color.DimGray
		};
		tableLayoutPanel.Controls.Add(control, 0, 3);
		tableLayoutPanel.SetColumnSpan(control, 3);
		rotcolBtn = new Button
		{
			Text = "Fix rotated collision pieces",
			Dock = DockStyle.Fill,
			BackColor = Color.FromArgb(70, 130, 180),
			ForeColor = Color.White,
			FlatStyle = FlatStyle.Flat
		};
		rotcolBtn.Click += async (object? s, EventArgs e) =>
		{
			await RunFixRotCollision();
		};
		tableLayoutPanel.Controls.Add(rotcolBtn, 1, 4);
		rotcolLog = MakeLog();
		tableLayoutPanel.Controls.Add(rotcolLog, 0, 5);
		tableLayoutPanel.SetColumnSpan(rotcolLog, 3);
		tabPage.Controls.Add(tableLayoutPanel);
		return tabPage;
	}

	private async Task RunFixRotCollision()
	{
		string merged = rotcolMerged.Text.Trim();
		string m13 = rotcolM13.Text.Trim();
		string outP = rotcolOut.Text.Trim();
		if (string.IsNullOrEmpty(merged) || !File.Exists(merged))
		{
			MessageBox.Show("Please select a valid [Map MSB] file.", "Notice");
			return;
		}
		if (string.IsNullOrEmpty(m13) || !File.Exists(m13))
		{
			MessageBox.Show("Please select a valid [Elden Ring original MSB(m13)] file.\n(Used to calculate building offsets; it is the Elden Ring map MSB used during merging)", "Notice");
			return;
		}
		if (string.IsNullOrEmpty(outP))
		{
			MessageBox.Show("Please specify the output location.", "Notice");
			return;
		}
		if (string.Equals(Path.GetFullPath(merged), Path.GetFullPath(outP), StringComparison.OrdinalIgnoreCase))
		{
			MessageBox.Show("The input and output cannot be the same file(to avoid overwriting the original file).", "Notice");
			return;
		}
		rotcolBtn.Enabled = false;
		rotcolLog.Clear();
		try
		{
			await Task.Run(() =>
			{
				MapCore.FixRotatedCollisionMsb(merged, m13, outP, (string msg) =>
				{
					Log(rotcolLog, msg);
				});
			});
			Log(rotcolLog, "");
			MessageBox.Show("Fix complete!\n\nOutput: " + outP + "\n\nPut it into map/mapstudio/ to overwrite the original MSB, then restart Smithbox to check whether the suffixed collisions are back in place.", "Done");
		}
		catch (Exception ex)
		{
			Log(rotcolLog, "");
			Log(rotcolLog, "!!! Error: " + ex.Message);
			if (ex.InnerException != null)
			{
				Log(rotcolLog, "    " + ex.InnerException.Message);
			}
			MessageBox.Show("Fix error:\n" + (ex.InnerException?.Message ?? ex.Message) + HintForError(ex), "Error");
		}
		finally
		{
			rotcolBtn.Enabled = true;
		}
	}

	private TabPage BuildFixNpcParamTab()
	{
		TabPage tabPage = new TabPage("Fix NPC params");
		TableLayoutPanel tableLayoutPanel = new TableLayoutPanel
		{
			Dock = DockStyle.Fill,
			Padding = new Padding(12),
			ColumnCount = 3,
			RowCount = 8
		};
		tableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 150f));
		tableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
		tableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 90f));
		tableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 40f));
		tableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 40f));
		tableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 40f));
		tableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 40f));
		tableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 70f));
		tableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 30f));
		tableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 44f));
		tableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));
		tableLayoutPanel.Controls.Add(new Label
		{
			Text = "Map MSB:",
			TextAlign = ContentAlignment.MiddleLeft,
			Dock = DockStyle.Fill
		}, 0, 0);
		npcMsb = new TextBox
		{
			Dock = DockStyle.Fill
		};
		npcMsb.TextChanged += (object? s, EventArgs e) =>
		{
			if (npcMsb.Text.Length > 0 && npcOut.Text.Length == 0)
			{
				string directoryName = Path.GetDirectoryName(npcMsb.Text);
				string fileName = Path.GetFileName(npcMsb.Text);
				npcOut.Text = (string.IsNullOrEmpty(directoryName) ? ("npcfix_" + fileName) : Path.Combine(directoryName, "npcfix_" + fileName));
			}
		};
		tableLayoutPanel.Controls.Add(npcMsb, 1, 0);
		Button button = new Button
		{
			Text = "Browse...",
			Dock = DockStyle.Fill
		};
		button.Click += (object? s, EventArgs e) =>
		{
			string text = PickOpen();
			if (text != null)
			{
				npcMsb.Text = text;
			}
		};
		tableLayoutPanel.Controls.Add(button, 2, 0);
		tableLayoutPanel.Controls.Add(new Label
		{
			Text = "Nightreign NpcParam.csv:",
			TextAlign = ContentAlignment.MiddleLeft,
			Dock = DockStyle.Fill
		}, 0, 1);
		npcCsv = new TextBox
		{
			Dock = DockStyle.Fill
		};
		tableLayoutPanel.Controls.Add(npcCsv, 1, 1);
		Button button2 = new Button
		{
			Text = "Browse...",
			Dock = DockStyle.Fill
		};
		button2.Click += (object? s, EventArgs e) =>
		{
			string text = PickOpen();
			if (text != null)
			{
				npcCsv.Text = text;
			}
		};
		tableLayoutPanel.Controls.Add(button2, 2, 1);
		tableLayoutPanel.Controls.Add(new Label
		{
			Text = "Nightreign NpcThinkParam.csv:",
			TextAlign = ContentAlignment.MiddleLeft,
			Dock = DockStyle.Fill
		}, 0, 2);
		npcThinkCsv = new TextBox
		{
			Dock = DockStyle.Fill
		};
		tableLayoutPanel.Controls.Add(npcThinkCsv, 1, 2);
		Button button3 = new Button
		{
			Text = "Browse...",
			Dock = DockStyle.Fill
		};
		button3.Click += (object? s, EventArgs e) =>
		{
			string text = PickOpen();
			if (text != null)
			{
				npcThinkCsv.Text = text;
			}
		};
		tableLayoutPanel.Controls.Add(button3, 2, 2);
		tableLayoutPanel.Controls.Add(new Label
		{
			Text = "Output location:",
			TextAlign = ContentAlignment.MiddleLeft,
			Dock = DockStyle.Fill
		}, 0, 3);
		npcOut = new TextBox
		{
			Dock = DockStyle.Fill
		};
		tableLayoutPanel.Controls.Add(npcOut, 1, 3);
		Button button4 = new Button
		{
			Text = "Browse...",
			Dock = DockStyle.Fill
		};
		button4.Click += (object? s, EventArgs e) =>
		{
			string text = PickSave(npcOut.Text);
			if (text != null)
			{
				npcOut.Text = text;
			}
		};
		tableLayoutPanel.Controls.Add(button4, 2, 3);
		Label control = new Label
		{
			Text = "Fixes monsters' NpcParamId / NpcThinkParamId to params that actually exist in Nightreign (after conversion they still point to Elden Ring params by default).\nRules: if the original param exists in the Nightreign table -> leave it unchanged; if not -> match by model ID (c8015 -> 80150000…), preferring the smallest ID whose rewardItemLot_2 is not empty.\nThose that match in neither table (Elden Ring-exclusive bosses, etc.) will be listed below as a notice, and keep their original values. The first column of the CSV is the ID, and the first row is the header.",
			TextAlign = ContentAlignment.TopLeft,
			Dock = DockStyle.Fill,
			ForeColor = Color.DimGray
		};
		tableLayoutPanel.Controls.Add(control, 0, 4);
		tableLayoutPanel.SetColumnSpan(control, 3);
		npcSecondPass = new CheckBox
		{
			Text = "Secondary matching (upgrade monsters that are already set but have an empty rewardItemLot_2 to a same-model variant that has rewards)",
			Dock = DockStyle.Fill,
			TextAlign = ContentAlignment.MiddleLeft
		};
		tableLayoutPanel.Controls.Add(npcSecondPass, 0, 5);
		tableLayoutPanel.SetColumnSpan(npcSecondPass, 3);
		npcBtn = new Button
		{
			Text = "Match and fix NPC params",
			Dock = DockStyle.Fill,
			BackColor = Color.FromArgb(70, 130, 180),
			ForeColor = Color.White,
			FlatStyle = FlatStyle.Flat
		};
		npcBtn.Click += async (object? s, EventArgs e) =>
		{
			await RunFixNpcParam();
		};
		tableLayoutPanel.Controls.Add(npcBtn, 1, 6);
		npcLog = MakeLog();
		tableLayoutPanel.Controls.Add(npcLog, 0, 7);
		tableLayoutPanel.SetColumnSpan(npcLog, 3);
		tabPage.Controls.Add(tableLayoutPanel);
		return tabPage;
	}

	private async Task RunFixNpcParam()
	{
		string msb = npcMsb.Text.Trim();
		string csv = npcCsv.Text.Trim();
		string think = npcThinkCsv.Text.Trim();
		string outP = npcOut.Text.Trim();
		if (string.IsNullOrEmpty(msb) || !File.Exists(msb))
		{
			MessageBox.Show("Please select a valid [Map MSB] file.", "Notice");
			return;
		}
		if (string.IsNullOrEmpty(csv) || !File.Exists(csv))
		{
			MessageBox.Show("Please select a valid [Nightreign NpcParam.csv].", "Notice");
			return;
		}
		if (string.IsNullOrEmpty(think) || !File.Exists(think))
		{
			MessageBox.Show("Please select a valid [Nightreign NpcThinkParam.csv].", "Notice");
			return;
		}
		if (string.IsNullOrEmpty(outP))
		{
			MessageBox.Show("Please specify the output location.", "Notice");
			return;
		}
		if (string.Equals(Path.GetFullPath(msb), Path.GetFullPath(outP), StringComparison.OrdinalIgnoreCase))
		{
			MessageBox.Show("The input and output cannot be the same file(to avoid overwriting the original file).", "Notice");
			return;
		}
		npcBtn.Enabled = false;
		npcLog.Clear();
		bool second = npcSecondPass.Checked;
		try
		{
			await Task.Run(() =>
			{
				MapCore.FixNpcParams(msb, csv, think, outP, second, (string m) =>
				{
					Log(npcLog, m);
				});
			});
			Log(npcLog, "");
			string text = (second ? "Secondary matching(upgrade to variant with rewards)" : "First matching");
			MessageBox.Show("Fix complete [" + text + "]!\n\nOutput: " + outP + "\n\nIf the log contains models marked \"no match found\", those are Elden Ring-exclusive monsters that Nightreign does not have, and need to be handled manually.\nThe rest have been automatically mapped to Nightreign params; put it into map/mapstudio/ to overwrite the original MSB.", "Done");
		}
		catch (Exception ex)
		{
			Log(npcLog, "");
			Log(npcLog, "!!! Error: " + ex.Message);
			if (ex.InnerException != null)
			{
				Log(npcLog, "    " + ex.InnerException.Message);
			}
			MessageBox.Show("Fix error:\n" + (ex.InnerException?.Message ?? ex.Message) + HintForError(ex), "Error");
		}
		finally
		{
			npcBtn.Enabled = true;
		}
	}

	private TabPage BuildAutoAlignTab()
	{
		TabPage tabPage = new TabPage("Auto-align collision");
		TableLayoutPanel tableLayoutPanel = new TableLayoutPanel
		{
			Dock = DockStyle.Fill,
			Padding = new Padding(12),
			ColumnCount = 3,
			RowCount = 8
		};
		tableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 140f));
		tableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
		tableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 90f));
		for (int i = 0; i < 6; i++)
		{
			tableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 36f));
		}
		tableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 44f));
		tableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));
		tableLayoutPanel.Controls.Add(new Label
		{
			Text = "Elden Ring h collision(.hkxbhd)",
			TextAlign = ContentAlignment.MiddleLeft,
			Dock = DockStyle.Fill
		}, 0, 0);
		aaAddH = new TextBox
		{
			Dock = DockStyle.Fill
		};
		tableLayoutPanel.Controls.Add(aaAddH, 1, 0);
		Button button = new Button
		{
			Text = "Browse...",
			Dock = DockStyle.Fill
		};
		button.Click += (object? s, EventArgs e) =>
		{
			string text = PickOpenF("Collision pack (*.hkxbhd)|*.hkxbhd|All files (*.*)|*.*");
			if (text != null)
			{
				aaAddH.Text = text;
			}
		};
		tableLayoutPanel.Controls.Add(button, 2, 0);
		tableLayoutPanel.Controls.Add(new Label
		{
			Text = "Nightreign h collision(.hkxbhd):",
			TextAlign = ContentAlignment.MiddleLeft,
			Dock = DockStyle.Fill
		}, 0, 1);
		aaBaseH = new TextBox
		{
			Dock = DockStyle.Fill
		};
		tableLayoutPanel.Controls.Add(aaBaseH, 1, 1);
		Button button2 = new Button
		{
			Text = "Browse...",
			Dock = DockStyle.Fill
		};
		button2.Click += (object? s, EventArgs e) =>
		{
			string text = PickOpenF("Collision pack (*.hkxbhd)|*.hkxbhd|All files (*.*)|*.*");
			if (text != null)
			{
				aaBaseH.Text = text;
			}
		};
		tableLayoutPanel.Controls.Add(button2, 2, 1);
		tableLayoutPanel.Controls.Add(new Label
		{
			Text = "Merged MSB(m60):",
			TextAlign = ContentAlignment.MiddleLeft,
			Dock = DockStyle.Fill
		}, 0, 2);
		aaMsbMerged = new TextBox
		{
			Dock = DockStyle.Fill
		};
		tableLayoutPanel.Controls.Add(aaMsbMerged, 1, 2);
		Button button3 = new Button
		{
			Text = "Browse...",
			Dock = DockStyle.Fill
		};
		button3.Click += (object? s, EventArgs e) =>
		{
			string text = PickOpenF("MSB map files (*.msb.dcx)|*.msb.dcx|All files (*.*)|*.*");
			if (text != null)
			{
				aaMsbMerged.Text = text;
			}
		};
		tableLayoutPanel.Controls.Add(button3, 2, 2);
		tableLayoutPanel.Controls.Add(new Label
		{
			Text = "Elden Ring original MSB(m13):",
			TextAlign = ContentAlignment.MiddleLeft,
			Dock = DockStyle.Fill
		}, 0, 3);
		aaMsbOrig = new TextBox
		{
			Dock = DockStyle.Fill
		};
		tableLayoutPanel.Controls.Add(aaMsbOrig, 1, 3);
		Button button4 = new Button
		{
			Text = "Browse...",
			Dock = DockStyle.Fill
		};
		button4.Click += (object? s, EventArgs e) =>
		{
			string text = PickOpenF("MSB map files (*.msb.dcx)|*.msb.dcx|All files (*.*)|*.*");
			if (text != null)
			{
				aaMsbOrig.Text = text;
			}
		};
		tableLayoutPanel.Controls.Add(button4, 2, 3);
		tableLayoutPanel.Controls.Add(new Label
		{
			Text = "Output directory:",
			TextAlign = ContentAlignment.MiddleLeft,
			Dock = DockStyle.Fill
		}, 0, 4);
		aaOutDir = new TextBox
		{
			Dock = DockStyle.Fill
		};
		tableLayoutPanel.Controls.Add(aaOutDir, 1, 4);
		Button button5 = new Button
		{
			Text = "Browse...",
			Dock = DockStyle.Fill
		};
		button5.Click += (object? s, EventArgs e) =>
		{
			string text = PickFolder();
			if (text != null)
			{
				aaOutDir.Text = text;
			}
		};
		tableLayoutPanel.Controls.Add(button5, 2, 4);
		Label control = new Label
		{
			Text = "Fully automatic: read both MSBs to calculate building offsets -> shift Elden Ring collision geometry -> merge into Nightreign collision -> output matching h60/l60.\nRequires oo2core_9_win64.dll (copy from the game's Game directory) placed next to the exe. The l collision is automatically paired from the same-named l file in the same directory.",
			TextAlign = ContentAlignment.MiddleLeft,
			Dock = DockStyle.Fill,
			ForeColor = Color.DimGray
		};
		tableLayoutPanel.Controls.Add(control, 0, 5);
		tableLayoutPanel.SetColumnSpan(control, 3);
		aaBtn = new Button
		{
			Text = "Auto-align and merge collision",
			Dock = DockStyle.Fill,
			BackColor = Color.FromArgb(60, 160, 110),
			ForeColor = Color.White,
			FlatStyle = FlatStyle.Flat
		};
		aaBtn.Click += async (object? s, EventArgs e) =>
		{
			await RunAutoAlign();
		};
		tableLayoutPanel.Controls.Add(aaBtn, 1, 6);
		aaLog = MakeLog();
		tableLayoutPanel.Controls.Add(aaLog, 0, 7);
		tableLayoutPanel.SetColumnSpan(aaLog, 3);
		tabPage.Controls.Add(tableLayoutPanel);
		return tabPage;
	}

	private async Task RunAutoAlign()
	{
		string addH = aaAddH.Text.Trim();
		string baseH = aaBaseH.Text.Trim();
		string msbM = aaMsbMerged.Text.Trim();
		string msbO = aaMsbOrig.Text.Trim();
		string outDir = aaOutDir.Text.Trim();
		if (string.IsNullOrEmpty(addH) || !File.Exists(addH))
		{
			MessageBox.Show("Please select the Elden Ring collision h.hkxbhd", "Notice");
			return;
		}
		if (string.IsNullOrEmpty(baseH) || !File.Exists(baseH))
		{
			MessageBox.Show("Please select the Nightreign collision h.hkxbhd", "Notice");
			return;
		}
		if (string.IsNullOrEmpty(msbM) || !File.Exists(msbM))
		{
			MessageBox.Show("Please select the merged MSB", "Notice");
			return;
		}
		if (string.IsNullOrEmpty(msbO) || !File.Exists(msbO))
		{
			MessageBox.Show("Please select the Elden Ring original MSB", "Notice");
			return;
		}
		if (string.IsNullOrEmpty(outDir))
		{
			MessageBox.Show("Please specify the output directory", "Notice");
			return;
		}
		aaBtn.Enabled = false;
		aaLog.Clear();
		try
		{
			await Task.Run(() =>
			{
				MapCore.AutoAlignAndMergeCollision(addH, baseH, msbM, msbO, outDir, (string m) =>
				{
					Log(aaLog, m);
				});
			});
			Log(aaLog, "");
			MessageBox.Show("Auto-align and merge complete!\n\nOutput directory: " + outDir + "\n\n1) Replace the h60/l60 .hkxbhd+.hkxbdt files into the mod's map folder.\n2) If the log outputs an MSB with \"_collision zeroed\", use it to replace your m60 MSB (to avoid double offset and crashes).\n\nSee the log below for details.", "Done");
		}
		catch (Exception ex)
		{
			Log(aaLog, "");
			Log(aaLog, "!!! Error: " + ex.Message);
			if (ex.InnerException != null)
			{
				Log(aaLog, "    " + ex.InnerException.Message);
			}
			MessageBox.Show("Error:\n" + (ex.InnerException?.Message ?? ex.Message), "Error");
		}
		finally
		{
			aaBtn.Enabled = true;
		}
	}

	private TabPage BuildMergeTab()
	{
		TabPage tabPage = new TabPage("Merge map");
		TableLayoutPanel tableLayoutPanel = new TableLayoutPanel
		{
			Dock = DockStyle.Fill,
			Padding = new Padding(12),
			ColumnCount = 3,
			RowCount = 8
		};
		tableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 130f));
		tableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
		tableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 90f));
		for (int i = 0; i < 4; i++)
		{
			tableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 40f));
		}
		tableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 32f));
		tableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 56f));
		tableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 44f));
		tableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));
		tableLayoutPanel.Controls.Add(new Label
		{
			Text = "Base map(Nightreign):",
			TextAlign = ContentAlignment.MiddleLeft,
			Dock = DockStyle.Fill
		}, 0, 0);
		mergeBase = new TextBox
		{
			Dock = DockStyle.Fill
		};
		mergeBase.TextChanged += (object? s, EventArgs e) =>
		{
			AutoFillMergeOut();
		};
		tableLayoutPanel.Controls.Add(mergeBase, 1, 0);
		Button button = new Button
		{
			Text = "Browse...",
			Dock = DockStyle.Fill
		};
		button.Click += (object? s, EventArgs e) =>
		{
			string text = PickOpen();
			if (text != null)
			{
				mergeBase.Text = text;
			}
		};
		tableLayoutPanel.Controls.Add(button, 2, 0);
		tableLayoutPanel.Controls.Add(new Label
		{
			Text = "Map to add(Elden Ring):",
			TextAlign = ContentAlignment.MiddleLeft,
			Dock = DockStyle.Fill
		}, 0, 1);
		mergeAdd = new TextBox
		{
			Dock = DockStyle.Fill
		};
		tableLayoutPanel.Controls.Add(mergeAdd, 1, 1);
		Button button2 = new Button
		{
			Text = "Browse...",
			Dock = DockStyle.Fill
		};
		button2.Click += (object? s, EventArgs e) =>
		{
			string text = PickOpen();
			if (text != null)
			{
				mergeAdd.Text = text;
			}
		};
		tableLayoutPanel.Controls.Add(button2, 2, 1);
		tableLayoutPanel.Controls.Add(new Label
		{
			Text = "Output location:",
			TextAlign = ContentAlignment.MiddleLeft,
			Dock = DockStyle.Fill
		}, 0, 2);
		mergeOut = new TextBox
		{
			Dock = DockStyle.Fill
		};
		tableLayoutPanel.Controls.Add(mergeOut, 1, 2);
		Button button3 = new Button
		{
			Text = "Browse...",
			Dock = DockStyle.Fill
		};
		button3.Click += (object? s, EventArgs e) =>
		{
			string text = PickSave(mergeOut.Text);
			if (text != null)
			{
				mergeOut.Text = text;
			}
		};
		tableLayoutPanel.Controls.Add(button3, 2, 2);
		tableLayoutPanel.Controls.Add(new Label
		{
			Text = "Collision remap table(optional):",
			TextAlign = ContentAlignment.MiddleLeft,
			Dock = DockStyle.Fill
		}, 0, 3);
		mergeCollRemap = new TextBox
		{
			Dock = DockStyle.Fill
		};
		tableLayoutPanel.Controls.Add(mergeCollRemap, 1, 3);
		Button button4 = new Button
		{
			Text = "Select file...",
			Dock = DockStyle.Fill
		};
		button4.Click += (object? s, EventArgs e) =>
		{
			string text = PickOpenF("Text files (*.txt)|*.txt|All files (*.*)|*.*");
			if (text != null)
			{
				mergeCollRemap.Text = text;
			}
		};
		tableLayoutPanel.Controls.Add(button4, 2, 3);
		mergeLinkEnemyGroup = new CheckBox
		{
			Text = "Automatically add ported enemies to the map's enemy group (fixes \"enemies are in the MSB but the game does not spawn them\"; automatically detects the most commonly used enemy group in the base)",
			Dock = DockStyle.Fill,
			TextAlign = ContentAlignment.MiddleLeft,
			Checked = true
		};
		tableLayoutPanel.Controls.Add(mergeLinkEnemyGroup, 1, 4);
		tableLayoutPanel.SetColumnSpan(mergeLinkEnemyGroup, 2);
		Label control = new Label
		{
			Dock = DockStyle.Fill,
			ForeColor = Color.DimGray,
			Text = "Merges the Elden Ring map into a Nightreign tile (the base stays unchanged; added entries with conflicting names are renamed).\nIf you are merging the map into an existing tile and want to bring the collision along: first use 'Merge map collision' to get the collision remap table, then select it here, so that the MSB's collision references line up."
		};
		tableLayoutPanel.Controls.Add(control, 1, 5);
		tableLayoutPanel.SetColumnSpan(control, 2);
		mergeBtn = new Button
		{
			Text = "Start merge",
			Dock = DockStyle.Fill,
			BackColor = Color.FromArgb(60, 150, 100),
			ForeColor = Color.White,
			FlatStyle = FlatStyle.Flat
		};
		mergeBtn.Click += async (object? s, EventArgs e) =>
		{
			await RunMerge();
		};
		tableLayoutPanel.Controls.Add(mergeBtn, 1, 6);
		mergeLog = MakeLog();
		tableLayoutPanel.Controls.Add(mergeLog, 0, 7);
		tableLayoutPanel.SetColumnSpan(mergeLog, 3);
		tabPage.Controls.Add(tableLayoutPanel);
		return tabPage;
	}

	private TabPage BuildAssetTab()
	{
		TabPage tabPage = new TabPage("Copy model assets (AEG/AET)");
		TableLayoutPanel tableLayoutPanel = new TableLayoutPanel
		{
			Dock = DockStyle.Fill,
			Padding = new Padding(12),
			ColumnCount = 3,
			RowCount = 6
		};
		tableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 130f));
		tableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
		tableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 90f));
		tableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 40f));
		tableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 40f));
		tableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 40f));
		tableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 70f));
		tableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 44f));
		tableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));
		tableLayoutPanel.Controls.Add(new Label
		{
			Text = "Nightreign map:",
			TextAlign = ContentAlignment.MiddleLeft,
			Dock = DockStyle.Fill
		}, 0, 0);
		assetMap = new TextBox
		{
			Dock = DockStyle.Fill
		};
		tableLayoutPanel.Controls.Add(assetMap, 1, 0);
		Button button = new Button
		{
			Text = "Browse...",
			Dock = DockStyle.Fill
		};
		button.Click += (object? s, EventArgs e) =>
		{
			string text = PickOpen();
			if (text != null)
			{
				assetMap.Text = text;
			}
		};
		tableLayoutPanel.Controls.Add(button, 2, 0);
		tableLayoutPanel.Controls.Add(new Label
		{
			Text = "Input directory (Elden Ring):",
			TextAlign = ContentAlignment.MiddleLeft,
			Dock = DockStyle.Fill
		}, 0, 1);
		assetInDir = new TextBox
		{
			Dock = DockStyle.Fill
		};
		tableLayoutPanel.Controls.Add(assetInDir, 1, 1);
		Button button2 = new Button
		{
			Text = "Select directory...",
			Dock = DockStyle.Fill
		};
		button2.Click += (object? s, EventArgs e) =>
		{
			string text = PickFolder();
			if (text != null)
			{
				assetInDir.Text = text;
			}
		};
		tableLayoutPanel.Controls.Add(button2, 2, 1);
		tableLayoutPanel.Controls.Add(new Label
		{
			Text = "Output directory (mod):",
			TextAlign = ContentAlignment.MiddleLeft,
			Dock = DockStyle.Fill
		}, 0, 2);
		assetOutDir = new TextBox
		{
			Dock = DockStyle.Fill
		};
		tableLayoutPanel.Controls.Add(assetOutDir, 1, 2);
		Button button3 = new Button
		{
			Text = "Select directory...",
			Dock = DockStyle.Fill
		};
		button3.Click += (object? s, EventArgs e) =>
		{
			string text = PickFolder();
			if (text != null)
			{
				assetOutDir.Text = text;
			}
		};
		tableLayoutPanel.Controls.Add(button3, 2, 2);
		tableLayoutPanel.Controls.Add(new Label
		{
			Text = "AET texture mode:",
			TextAlign = ContentAlignment.MiddleLeft,
			Dock = DockStyle.Fill
		}, 0, 3);
		FlowLayoutPanel flowLayoutPanel = new FlowLayoutPanel
		{
			Dock = DockStyle.Fill,
			FlowDirection = FlowDirection.TopDown,
			WrapContents = false,
			Margin = new Padding(0)
		};
		assetAetPrecise = new RadioButton
		{
			Text = "Exact: copy only the textures matching each building's ID (recommended, fixes too many textures being given)",
			AutoSize = true,
			Checked = true,
			Margin = new Padding(0, 1, 0, 1)
		};
		assetAetDeep = new RadioButton
		{
			Text = "Deep exact: additionally read the material database to fill in shared textures (more complete, slow, requires oo2core dll)",
			AutoSize = true,
			Margin = new Padding(0, 1, 0, 1)
		};
		assetAetWhole = new RadioButton
		{
			Text = "Whole group: copy the entire aet group in use (safest, lots of redundancy; use when missing textures appear white or purple)",
			AutoSize = true,
			Margin = new Padding(0, 1, 0, 1)
		};
		flowLayoutPanel.Controls.Add(assetAetPrecise);
		flowLayoutPanel.Controls.Add(assetAetDeep);
		flowLayoutPanel.Controls.Add(assetAetWhole);
		tableLayoutPanel.Controls.Add(flowLayoutPanel, 1, 3);
		tableLayoutPanel.SetColumnSpan(flowLayoutPanel, 2);
		FlowLayoutPanel flowLayoutPanel2 = new FlowLayoutPanel
		{
			Dock = DockStyle.Fill,
			FlowDirection = FlowDirection.LeftToRight,
			WrapContents = false
		};
		assetBtn = new Button
		{
			Text = "Scan and copy",
			Width = 140,
			Height = 34,
			BackColor = Color.FromArgb(150, 100, 60),
			ForeColor = Color.White,
			FlatStyle = FlatStyle.Flat
		};
		assetBtn.Click += async (object? s, EventArgs e) =>
		{
			await RunCopyAssets();
		};
		manifestBtn = new Button
		{
			Text = "Export list only",
			Width = 130,
			Height = 34,
			FlatStyle = FlatStyle.Flat
		};
		manifestBtn.Click += (object? s, EventArgs e) =>
		{
			RunExportManifest();
		};
		flowLayoutPanel2.Controls.Add(assetBtn);
		flowLayoutPanel2.Controls.Add(manifestBtn);
		tableLayoutPanel.Controls.Add(flowLayoutPanel2, 1, 4);
		assetLog = MakeLog();
		tableLayoutPanel.Controls.Add(assetLog, 0, 5);
		tableLayoutPanel.SetColumnSpan(assetLog, 3);
		tabPage.Controls.Add(tableLayoutPanel);
		return tabPage;
	}

	private TextBox MakeLog()
	{
		return new TextBox
		{
			Multiline = true,
			ReadOnly = true,
			ScrollBars = ScrollBars.Vertical,
			Dock = DockStyle.Fill,
			BackColor = Color.FromArgb(30, 30, 30),
			ForeColor = Color.FromArgb(220, 220, 220),
			Font = new Font("Consolas", 9f),
			WordWrap = false
		};
	}

	private string PickFolder()
	{
		FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog();
		try
		{
			return (folderBrowserDialog.ShowDialog() == DialogResult.OK) ? folderBrowserDialog.SelectedPath : null;
		}
		finally
		{
			((IDisposable)(object)folderBrowserDialog)?.Dispose();
		}
	}

	private string PickOpenF(string filter)
	{
		OpenFileDialog openFileDialog = new OpenFileDialog
		{
			Filter = filter
		};
		try
		{
			return (openFileDialog.ShowDialog() == DialogResult.OK) ? openFileDialog.FileName : null;
		}
		finally
		{
			((IDisposable)(object)openFileDialog)?.Dispose();
		}
	}

	private string PickSaveF(string filter, string name)
	{
		SaveFileDialog saveFileDialog = new SaveFileDialog
		{
			Filter = filter,
			FileName = name
		};
		try
		{
			return (saveFileDialog.ShowDialog() == DialogResult.OK) ? saveFileDialog.FileName : null;
		}
		finally
		{
			((IDisposable)(object)saveFileDialog)?.Dispose();
		}
	}

	private TabPage BuildEventTab()
	{
		TabPage tabPage = new TabPage("Merge events(EMEVD)");
		TableLayoutPanel tableLayoutPanel = new TableLayoutPanel
		{
			Dock = DockStyle.Fill,
			Padding = new Padding(12),
			ColumnCount = 3,
			RowCount = 7
		};
		tableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 150f));
		tableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
		tableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 90f));
		for (int i = 0; i < 4; i++)
		{
			tableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 40f));
		}
		tableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 56f));
		tableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 44f));
		tableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));
		tableLayoutPanel.Controls.Add(new Label
		{
			Text = "Base events(Nightreign):",
			TextAlign = ContentAlignment.MiddleLeft,
			Dock = DockStyle.Fill
		}, 0, 0);
		eventBase = new TextBox
		{
			Dock = DockStyle.Fill
		};
		tableLayoutPanel.Controls.Add(eventBase, 1, 0);
		Button button = new Button
		{
			Text = "Select file...",
			Dock = DockStyle.Fill
		};
		button.Click += (object? s, EventArgs e) =>
		{
			string text = PickOpenF("Event files (*.emevd.dcx)|*.emevd.dcx|All files (*.*)|*.*");
			if (text != null)
			{
				eventBase.Text = text;
			}
		};
		tableLayoutPanel.Controls.Add(button, 2, 0);
		tableLayoutPanel.Controls.Add(new Label
		{
			Text = "Events to add(Elden Ring):",
			TextAlign = ContentAlignment.MiddleLeft,
			Dock = DockStyle.Fill
		}, 0, 1);
		eventAdd = new TextBox
		{
			Dock = DockStyle.Fill
		};
		tableLayoutPanel.Controls.Add(eventAdd, 1, 1);
		Button button2 = new Button
		{
			Text = "Select file...",
			Dock = DockStyle.Fill
		};
		button2.Click += (object? s, EventArgs e) =>
		{
			string text = PickOpenF("Event files (*.emevd.dcx)|*.emevd.dcx|All files (*.*)|*.*");
			if (text != null)
			{
				eventAdd.Text = text;
			}
		};
		tableLayoutPanel.Controls.Add(button2, 2, 1);
		tableLayoutPanel.Controls.Add(new Label
		{
			Text = "Output:",
			TextAlign = ContentAlignment.MiddleLeft,
			Dock = DockStyle.Fill
		}, 0, 2);
		eventOut = new TextBox
		{
			Dock = DockStyle.Fill
		};
		tableLayoutPanel.Controls.Add(eventOut, 1, 2);
		Button button3 = new Button
		{
			Text = "Save as...",
			Dock = DockStyle.Fill
		};
		button3.Click += (object? s, EventArgs e) =>
		{
			string text = PickSaveF("Event files (*.emevd.dcx)|*.emevd.dcx|All files (*.*)|*.*", "merged.emevd.dcx");
			if (text != null)
			{
				eventOut.Text = text;
			}
		};
		tableLayoutPanel.Controls.Add(button3, 2, 2);
		tableLayoutPanel.Controls.Add(new Label
		{
			Text = "common_func remap table\n(optional):",
			TextAlign = ContentAlignment.MiddleLeft,
			Dock = DockStyle.Fill
		}, 0, 3);
		eventRemap = new TextBox
		{
			Dock = DockStyle.Fill
		};
		tableLayoutPanel.Controls.Add(eventRemap, 1, 3);
		Button button4 = new Button
		{
			Text = "Select file...",
			Dock = DockStyle.Fill
		};
		button4.Click += (object? s, EventArgs e) =>
		{
			string text = PickOpenF("Text files (*.txt)|*.txt|All files (*.*)|*.*");
			if (text != null)
			{
				eventRemap.Text = text;
			}
		};
		tableLayoutPanel.Controls.Add(button4, 2, 3);
		Label control = new Label
		{
			Dock = DockStyle.Fill,
			ForeColor = Color.DimGray,
			Text = "Uses the Nightreign map events as the base and merges the Elden Ring map events into it: Elden Ring events will be added and initialized (merging event 0 from both sides). Elden Ring events with conflicting IDs are skipped automatically.\nRemap table (optional): generate it first on the 'Merge common_func' tab on the right, then select it; calls from Elden Ring events to conflicting common_func will automatically point to the renumbered versions.\nRequires oo2core_9_win64.dll to read and write KRAK compression."
		};
		tableLayoutPanel.Controls.Add(control, 1, 4);
		tableLayoutPanel.SetColumnSpan(control, 2);
		eventBtn = new Button
		{
			Text = "Merge events",
			Dock = DockStyle.Fill,
			BackColor = Color.FromArgb(70, 110, 90),
			ForeColor = Color.White,
			FlatStyle = FlatStyle.Flat
		};
		eventBtn.Click += async (object? s, EventArgs e) =>
		{
			await RunMergeEvent();
		};
		tableLayoutPanel.Controls.Add(eventBtn, 1, 5);
		eventLog = MakeLog();
		tableLayoutPanel.Controls.Add(eventLog, 0, 6);
		tableLayoutPanel.SetColumnSpan(eventLog, 3);
		tabPage.Controls.Add(tableLayoutPanel);
		return tabPage;
	}

	private async Task RunMergeEvent()
	{
		string b = eventBase.Text.Trim();
		string a = eventAdd.Text.Trim();
		string o = eventOut.Text.Trim();
		string text = eventRemap.Text.Trim();
		if (string.IsNullOrEmpty(b) || !File.Exists(b))
		{
			MessageBox.Show("Please select the base event file(Nightreign .emevd.dcx).", "Notice");
			return;
		}
		if (string.IsNullOrEmpty(a) || !File.Exists(a))
		{
			MessageBox.Show("Please select the event file to add(Elden Ring .emevd.dcx).", "Notice");
			return;
		}
		if (string.IsNullOrEmpty(o))
		{
			MessageBox.Show("Please select the output location.", "Notice");
			return;
		}
		if (!string.IsNullOrEmpty(text) && !File.Exists(text))
		{
			MessageBox.Show("The remap table file does not exist(leave empty to not use one).", "Notice");
			return;
		}
		eventBtn.Enabled = false;
		eventLog.Clear();
		try
		{
			string remapArg = (string.IsNullOrEmpty(text) ? null : text);
			await Task.Run(() =>
			{
				MapCore.MergeEmevd(b, a, o, (string m) =>
				{
					Log(eventLog, m);
				}, remapArg);
			});
			Log(eventLog, "");
			MessageBox.Show("Event merge complete!\n\nOutput: " + o, "Done");
		}
		catch (Exception ex)
		{
			Log(eventLog, "");
			Log(eventLog, "!!! Error: " + ex.Message);
			MessageBox.Show("Fix error:\n" + (ex.InnerException?.Message ?? ex.Message) + HintForError(ex), "Error");
		}
		finally
		{
			eventBtn.Enabled = true;
		}
	}

	private TabPage BuildMapCollisionTab()
	{
		TabPage tabPage = new TabPage("Merge map collision");
		TableLayoutPanel p = new TableLayoutPanel
		{
			Dock = DockStyle.Fill,
			Padding = new Padding(12),
			ColumnCount = 3,
			RowCount = 11
		};
		p.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 175f));
		p.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
		p.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 90f));
		for (int i = 0; i < 7; i++)
		{
			p.RowStyles.Add(new RowStyle(SizeType.Absolute, 38f));
		}
		p.RowStyles.Add(new RowStyle(SizeType.Absolute, 76f));
		p.RowStyles.Add(new RowStyle(SizeType.Absolute, 30f));
		p.RowStyles.Add(new RowStyle(SizeType.Absolute, 44f));
		p.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));
		mcBaseH = new TextBox
		{
			Dock = DockStyle.Fill
		};
		Row(0, "Nightreign high collision .hkxbhd:", null, mcBaseH, open: true, null);
		mcAddH = new TextBox
		{
			Dock = DockStyle.Fill
		};
		Row(1, "Elden Ring high collision .hkxbhd:", null, mcAddH, open: true, null);
		mcBaseL = new TextBox
		{
			Dock = DockStyle.Fill
		};
		Row(2, "Nightreign low collision .hkxbhd:", null, mcBaseL, open: true, null);
		mcAddL = new TextBox
		{
			Dock = DockStyle.Fill
		};
		Row(3, "Elden Ring low collision .hkxbhd:", null, mcAddL, open: true, null);
		mcOutH = new TextBox
		{
			Dock = DockStyle.Fill
		};
		Row(4, "Output high collision .hkxbhd:", null, mcOutH, open: false, "h_merged.hkxbhd");
		mcOutL = new TextBox
		{
			Dock = DockStyle.Fill
		};
		Row(5, "Output low collision .hkxbhd:", null, mcOutL, open: false, "l_merged.hkxbhd");
		mcRemap = new TextBox
		{
			Dock = DockStyle.Fill
		};
		Row(6, "Output collision remap table:", null, mcRemap, open: false, "Collision remap table.txt");
		Label control = new Label
		{
			Dock = DockStyle.Fill,
			ForeColor = Color.DimGray,
			Text = "Merges Elden Ring collision into Nightreign tile collision. The map prefixes of Elden Ring entries (m13_../h13_..) are automatically changed to Nightreign's (m60_../h60_..), keeping the numbers.\nOn the Nightreign side, please select the [original] tile collision (not one you have already replaced). Just select the .hkxbhd; the same-named .hkxbdt is read and written automatically together with it, and the compendium of Nightreign is kept (type table hashes are identical, so it is compatible).\nElden Ring numbers (0xxxxx) and Nightreign numbers (such as 4337xx) usually do not collide→the MSB does not need changes and the remap table is empty; only on a real collision are they renumbered and the table written.\nWorkflow: no collision→use the merged collision archive directly; collision→① merge here to get the remap table →② select the table in 'Merge map'. (Low collision l can be left empty; HKX is not decompressed, oo2core is not needed.)"
		};
		p.Controls.Add(control, 1, 7);
		p.SetColumnSpan(control, 2);
		mcDropHigh = new CheckBox
		{
			Text = "Exclude 9xxxxx high tiles (fixes Smithbox/game reporting \"compendium mismatch\" for 900000 etc.; cost: a few small areas at high places have no collision)",
			Dock = DockStyle.Fill,
			TextAlign = ContentAlignment.MiddleLeft,
			Checked = false
		};
		p.Controls.Add(mcDropHigh, 1, 8);
		p.SetColumnSpan(mcDropHigh, 2);
		mcBtn = new Button
		{
			Text = "Merge map collision",
			Dock = DockStyle.Fill,
			BackColor = Color.FromArgb(150, 100, 60),
			ForeColor = Color.White,
			FlatStyle = FlatStyle.Flat
		};
		mcBtn.Click += async (object? s, EventArgs e) =>
		{
			await RunMergeMapCollision();
		};
		p.Controls.Add(mcBtn, 1, 9);
		mcLog = MakeLog();
		p.Controls.Add(mcLog, 0, 10);
		p.SetColumnSpan(mcLog, 3);
		tabPage.Controls.Add(p);
		return tabPage;
		void Row(int r, string label, Action<TextBox> set, TextBox tb, bool open, string defName)
		{
			p.Controls.Add(new Label
			{
				Text = label,
				TextAlign = ContentAlignment.MiddleLeft,
				Dock = DockStyle.Fill
			}, 0, r);
			p.Controls.Add(tb, 1, r);
			Button button = new Button
			{
				Text = (open ? "选文件..." : "另存为..."),
				Dock = DockStyle.Fill
			};
			button.Click += (object? s, EventArgs e) =>
			{
				string text = (open ? PickOpenF("碰撞档案头 (*.hkxbhd)|*.hkxbhd|所有文件 (*.*)|*.*") : PickSaveF("碰撞档案头 (*.hkxbhd)|*.hkxbhd|所有文件 (*.*)|*.*", defName));
				if (text != null)
				{
					tb.Text = text;
				}
			};
			p.Controls.Add(button, 2, r);
		}
	}

	private async Task RunMergeMapCollision()
	{
		string bH = mcBaseH.Text.Trim();
		string aH = mcAddH.Text.Trim();
		string bL = mcBaseL.Text.Trim();
		string aL = mcAddL.Text.Trim();
		string oH = mcOutH.Text.Trim();
		string oL = mcOutL.Text.Trim();
		string rp = mcRemap.Text.Trim();
		if (string.IsNullOrEmpty(bH) || !File.Exists(bH))
		{
			MessageBox.Show("Please select the Nightreign high collision .hkxbhd.", "Notice");
			return;
		}
		if (string.IsNullOrEmpty(aH) || !File.Exists(aH))
		{
			MessageBox.Show("Please select the Elden Ring high collision .hkxbhd.", "Notice");
			return;
		}
		if (string.IsNullOrEmpty(oH))
		{
			MessageBox.Show("Please specify the output location for the high collision.", "Notice");
			return;
		}
		if (string.IsNullOrEmpty(rp))
		{
			MessageBox.Show("Please specify the output location for the remap table.", "Notice");
			return;
		}
		bool hasL = bL.Length > 0 && aL.Length > 0;
		if (hasL && string.IsNullOrEmpty(oL))
		{
			MessageBox.Show("A low collision input was given, so please also specify the output location for the low collision.", "Notice");
			return;
		}
		mcBtn.Enabled = false;
		mcLog.Clear();
		try
		{
			await Task.Run(() =>
			{
				MapCore.MergeMapCollision(bH, aH, hasL ? bL : null, hasL ? aL : null, oH, hasL ? oL : null, rp, (string m) =>
				{
					Log(mcLog, m);
				}, mcDropHigh.Checked);
			});
			Log(mcLog, "");
			MessageBox.Show("Collision archive merge complete!\n\nOutput: " + oH + (hasL ? ("\n" + oL) : "") + "\nRemap table: " + rp + "\n\nNext step: go to 'Merge map' and select this collision remap table.", "Done");
		}
		catch (Exception ex)
		{
			Log(mcLog, "");
			Log(mcLog, "!!! Error: " + ex.Message);
			if (ex.InnerException != null)
			{
				Log(mcLog, "    " + ex.InnerException.Message);
			}
			MessageBox.Show("Fix error:\n" + (ex.InnerException?.Message ?? ex.Message) + HintForError(ex), "Error");
		}
		finally
		{
			mcBtn.Enabled = true;
		}
	}

	private TabPage BuildCommonFuncTab()
	{
		TabPage tabPage = new TabPage("Merge common_func");
		TableLayoutPanel tableLayoutPanel = new TableLayoutPanel
		{
			Dock = DockStyle.Fill,
			Padding = new Padding(12),
			ColumnCount = 3,
			RowCount = 7
		};
		tableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 170f));
		tableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
		tableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 90f));
		for (int i = 0; i < 4; i++)
		{
			tableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 40f));
		}
		tableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 64f));
		tableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 44f));
		tableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));
		tableLayoutPanel.Controls.Add(new Label
		{
			Text = "Nightreign common_func:",
			TextAlign = ContentAlignment.MiddleLeft,
			Dock = DockStyle.Fill
		}, 0, 0);
		cfBase = new TextBox
		{
			Dock = DockStyle.Fill
		};
		tableLayoutPanel.Controls.Add(cfBase, 1, 0);
		Button button = new Button
		{
			Text = "Select file...",
			Dock = DockStyle.Fill
		};
		button.Click += (object? s, EventArgs e) =>
		{
			string text = PickOpenF("Event files (*.emevd.dcx)|*.emevd.dcx|All files (*.*)|*.*");
			if (text != null)
			{
				cfBase.Text = text;
			}
		};
		tableLayoutPanel.Controls.Add(button, 2, 0);
		tableLayoutPanel.Controls.Add(new Label
		{
			Text = "Elden Ring common_func:",
			TextAlign = ContentAlignment.MiddleLeft,
			Dock = DockStyle.Fill
		}, 0, 1);
		cfAdd = new TextBox
		{
			Dock = DockStyle.Fill
		};
		tableLayoutPanel.Controls.Add(cfAdd, 1, 1);
		Button button2 = new Button
		{
			Text = "Select file...",
			Dock = DockStyle.Fill
		};
		button2.Click += (object? s, EventArgs e) =>
		{
			string text = PickOpenF("Event files (*.emevd.dcx)|*.emevd.dcx|All files (*.*)|*.*");
			if (text != null)
			{
				cfAdd.Text = text;
			}
		};
		tableLayoutPanel.Controls.Add(button2, 2, 1);
		tableLayoutPanel.Controls.Add(new Label
		{
			Text = "Output common_func:",
			TextAlign = ContentAlignment.MiddleLeft,
			Dock = DockStyle.Fill
		}, 0, 2);
		cfOut = new TextBox
		{
			Dock = DockStyle.Fill
		};
		tableLayoutPanel.Controls.Add(cfOut, 1, 2);
		Button button3 = new Button
		{
			Text = "Save as...",
			Dock = DockStyle.Fill
		};
		button3.Click += (object? s, EventArgs e) =>
		{
			string text = PickSaveF("Event files (*.emevd.dcx)|*.emevd.dcx|All files (*.*)|*.*", "common_func.emevd.dcx");
			if (text != null)
			{
				cfOut.Text = text;
			}
		};
		tableLayoutPanel.Controls.Add(button3, 2, 2);
		tableLayoutPanel.Controls.Add(new Label
		{
			Text = "Output remap table:",
			TextAlign = ContentAlignment.MiddleLeft,
			Dock = DockStyle.Fill
		}, 0, 3);
		cfRemap = new TextBox
		{
			Dock = DockStyle.Fill
		};
		tableLayoutPanel.Controls.Add(cfRemap, 1, 3);
		Button button4 = new Button
		{
			Text = "Save as...",
			Dock = DockStyle.Fill
		};
		button4.Click += (object? s, EventArgs e) =>
		{
			string text = PickSaveF("Text files (*.txt)|*.txt|All files (*.*)|*.*", "common_func remap table.txt");
			if (text != null)
			{
				cfRemap.Text = text;
			}
		};
		tableLayoutPanel.Controls.Add(button4, 2, 3);
		Label control = new Label
		{
			Dock = DockStyle.Fill,
			ForeColor = Color.DimGray,
			Text = "Merges Elden Ring's common_func (common function library) into Nightreign's. Functions with the same ID but different functionality on the two sides will conflict——\nConflicting Elden Ring functions will be [renumbered] to free IDs, and a 'remap table' will be written out.\nWorkflow: ① merge common_func here to get the remap table → ② on the 'Merge events' tab, select the remap table when merging maps.\nRequires oo2core_9_win64.dll."
		};
		tableLayoutPanel.Controls.Add(control, 1, 4);
		tableLayoutPanel.SetColumnSpan(control, 2);
		cfBtn = new Button
		{
			Text = "Merge common_func",
			Dock = DockStyle.Fill,
			BackColor = Color.FromArgb(90, 90, 130),
			ForeColor = Color.White,
			FlatStyle = FlatStyle.Flat
		};
		cfBtn.Click += async (object? s, EventArgs e) =>
		{
			await RunMergeCommonFunc();
		};
		tableLayoutPanel.Controls.Add(cfBtn, 1, 5);
		cfLog = MakeLog();
		tableLayoutPanel.Controls.Add(cfLog, 0, 6);
		tableLayoutPanel.SetColumnSpan(cfLog, 3);
		tabPage.Controls.Add(tableLayoutPanel);
		return tabPage;
	}

	private async Task RunMergeCommonFunc()
	{
		string b = cfBase.Text.Trim();
		string a = cfAdd.Text.Trim();
		string o = cfOut.Text.Trim();
		string rm = cfRemap.Text.Trim();
		if (string.IsNullOrEmpty(b) || !File.Exists(b))
		{
			MessageBox.Show("Please select the Nightreign common_func.emevd.dcx.", "Notice");
			return;
		}
		if (string.IsNullOrEmpty(a) || !File.Exists(a))
		{
			MessageBox.Show("Please select the Elden Ring common_func.emevd.dcx.", "Notice");
			return;
		}
		if (string.IsNullOrEmpty(o))
		{
			MessageBox.Show("Please select the output location for common_func.", "Notice");
			return;
		}
		if (string.IsNullOrEmpty(rm))
		{
			MessageBox.Show("Please select the output location for the remap table.", "Notice");
			return;
		}
		cfBtn.Enabled = false;
		cfLog.Clear();
		try
		{
			await Task.Run(() =>
			{
				MapCore.MergeCommonFunc(b, a, o, rm, (string m) =>
				{
					Log(cfLog, m);
				});
			});
			Log(cfLog, "");
			MessageBox.Show("common_func merge complete!\n\nMerged: " + o + "\nRemap table: " + rm + "\n\nNext step: on the 'Merge events' tab, select this remap table when merging maps.", "Done");
		}
		catch (Exception ex)
		{
			Log(cfLog, "");
			Log(cfLog, "!!! Error: " + ex.Message);
			MessageBox.Show("Fix error:\n" + (ex.InnerException?.Message ?? ex.Message) + HintForError(ex), "Error");
		}
		finally
		{
			cfBtn.Enabled = true;
		}
	}

	private TabPage BuildVbsTab()
	{
		TabPage tabPage = new TabPage("Batch change vertex structure (VBS)");
		TableLayoutPanel tableLayoutPanel = new TableLayoutPanel
		{
			Dock = DockStyle.Fill,
			Padding = new Padding(12),
			ColumnCount = 3,
			RowCount = 5
		};
		tableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 130f));
		tableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
		tableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 90f));
		tableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 40f));
		tableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 40f));
		tableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 70f));
		tableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 44f));
		tableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));
		tableLayoutPanel.Controls.Add(new Label
		{
			Text = "Input directory(AEG):",
			TextAlign = ContentAlignment.MiddleLeft,
			Dock = DockStyle.Fill
		}, 0, 0);
		vbsInDir = new TextBox
		{
			Dock = DockStyle.Fill
		};
		tableLayoutPanel.Controls.Add(vbsInDir, 1, 0);
		Button button = new Button
		{
			Text = "Select directory...",
			Dock = DockStyle.Fill
		};
		button.Click += (object? s, EventArgs e) =>
		{
			string text = PickFolder();
			if (text != null)
			{
				vbsInDir.Text = text;
			}
		};
		tableLayoutPanel.Controls.Add(button, 2, 0);
		tableLayoutPanel.Controls.Add(new Label
		{
			Text = "Output directory:",
			TextAlign = ContentAlignment.MiddleLeft,
			Dock = DockStyle.Fill
		}, 0, 1);
		vbsOutDir = new TextBox
		{
			Dock = DockStyle.Fill
		};
		tableLayoutPanel.Controls.Add(vbsOutDir, 1, 1);
		Button button2 = new Button
		{
			Text = "Select directory...",
			Dock = DockStyle.Fill
		};
		button2.Click += (object? s, EventArgs e) =>
		{
			string text = PickFolder();
			if (text != null)
			{
				vbsOutDir.Text = text;
			}
		};
		tableLayoutPanel.Controls.Add(button2, 2, 1);
		Label control = new Label
		{
			Dock = DockStyle.Fill,
			ForeColor = Color.DimGray,
			Text = "Recursively traverses all AEGxxx_xxx.geombnd(.dcx) in the input directory and changes the vertex buffer structure of each model's FLVER to the [Nightreign native format]:\n Each attribute in its own separate buffer: Position(s0)·Normal(s1)·Tangent(s2)·BoneIndices(s4); UV/vertex colors are placed in s7/s8/s9 as needed; FLVER version→0x20021.\n It [adapts automatically] to each mesh's number of UV sets and whether it has vertex colors (exactly following the layout of the original Nightreign assets), so no UVs/vertex colors are lost.\nOnly the vertex structure is changed; materials/faces/bones are untouched. Requires oo2core_9_win64.dll placed next to this program (to read and write KRAK)."
		};
		tableLayoutPanel.Controls.Add(control, 1, 2);
		tableLayoutPanel.SetColumnSpan(control, 2);
		vbsBtn = new Button
		{
			Text = "Start batch VBS change",
			Dock = DockStyle.Fill,
			BackColor = Color.FromArgb(120, 70, 140),
			ForeColor = Color.White,
			FlatStyle = FlatStyle.Flat
		};
		vbsBtn.Click += async (object? s, EventArgs e) =>
		{
			await RunVbs();
		};
		tableLayoutPanel.Controls.Add(vbsBtn, 1, 3);
		vbsLog = MakeLog();
		tableLayoutPanel.Controls.Add(vbsLog, 0, 4);
		tableLayoutPanel.SetColumnSpan(vbsLog, 3);
		tabPage.Controls.Add(tableLayoutPanel);
		return tabPage;
	}

	private TabPage BuildMapPieceTab()
	{
		TabPage tabPage = new TabPage("Convert map tiles");
		TableLayoutPanel tableLayoutPanel = new TableLayoutPanel
		{
			Dock = DockStyle.Fill,
			Padding = new Padding(12),
			ColumnCount = 3,
			RowCount = 6
		};
		tableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 150f));
		tableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
		tableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 90f));
		tableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 40f));
		tableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 40f));
		tableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 40f));
		tableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 96f));
		tableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 44f));
		tableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));
		tableLayoutPanel.Controls.Add(new Label
		{
			Text = "Elden Ring mapbnd directory:",
			TextAlign = ContentAlignment.MiddleLeft,
			Dock = DockStyle.Fill
		}, 0, 0);
		mpInDir = new TextBox
		{
			Dock = DockStyle.Fill
		};
		tableLayoutPanel.Controls.Add(mpInDir, 1, 0);
		Button button = new Button
		{
			Text = "Select directory...",
			Dock = DockStyle.Fill
		};
		button.Click += (object? s, EventArgs e) =>
		{
			string text = PickFolder();
			if (text != null)
			{
				mpInDir.Text = text;
			}
		};
		tableLayoutPanel.Controls.Add(button, 2, 0);
		tableLayoutPanel.Controls.Add(new Label
		{
			Text = "Output directory:",
			TextAlign = ContentAlignment.MiddleLeft,
			Dock = DockStyle.Fill
		}, 0, 1);
		mpOutDir = new TextBox
		{
			Dock = DockStyle.Fill
		};
		tableLayoutPanel.Controls.Add(mpOutDir, 1, 1);
		Button button2 = new Button
		{
			Text = "Select directory...",
			Dock = DockStyle.Fill
		};
		button2.Click += (object? s, EventArgs e) =>
		{
			string text = PickFolder();
			if (text != null)
			{
				mpOutDir.Text = text;
			}
		};
		tableLayoutPanel.Controls.Add(button2, 2, 1);
		tableLayoutPanel.Controls.Add(new Label
		{
			Text = "Nightreign target map ID:",
			TextAlign = ContentAlignment.MiddleLeft,
			Dock = DockStyle.Fill
		}, 0, 2);
		mpMapId = new TextBox
		{
			Dock = DockStyle.Fill,
			Text = "60_43_37_00"
		};
		tableLayoutPanel.Controls.Add(mpMapId, 1, 2);
		Label control = new Label
		{
			Dock = DockStyle.Fill,
			ForeColor = Color.DimGray,
			Text = "Converts Elden Ring map tiles (mapbnd) to Nightreign format——fixes \"GPU driver crashes (nvwgf2umx.dll) when rendering buildings after entering the map\".\nFor the building FLVER in each mXX_XX_XX_XX_NNNNNN.mapbnd.dcx: Elden Ring vertex format→Nightreign (separate buffers, 0x20021, vertex colors dropped),\nand changes the map prefix in the internal paths and file names to the target ID above (e.g. m13_00_00_00 → m60_43_37_00, keeping the number NNNNNN).\nFor the target map ID, enter the Nightreign tile you want to merge into (default 60_43_37_00). Requires oo2core_9_win64.dll placed next to this program.\nAfter conversion, copy all the output m60_..._NNNNNN.mapbnd.dcx files into the m60 map folder, and the buildings will display normally without crashing."
		};
		tableLayoutPanel.Controls.Add(control, 1, 3);
		tableLayoutPanel.SetColumnSpan(control, 2);
		mpBtn = new Button
		{
			Text = "Convert map tiles",
			Dock = DockStyle.Fill,
			BackColor = Color.FromArgb(70, 110, 90),
			ForeColor = Color.White,
			FlatStyle = FlatStyle.Flat
		};
		mpBtn.Click += async (object? s, EventArgs e) =>
		{
			await RunConvertMapPieces();
		};
		tableLayoutPanel.Controls.Add(mpBtn, 1, 4);
		mpLog = MakeLog();
		tableLayoutPanel.Controls.Add(mpLog, 0, 5);
		tableLayoutPanel.SetColumnSpan(mpLog, 3);
		tabPage.Controls.Add(tableLayoutPanel);
		return tabPage;
	}

	private async Task RunConvertMapPieces()
	{
		string inDir = mpInDir.Text.Trim();
		string outDir = mpOutDir.Text.Trim();
		string mapId = mpMapId.Text.Trim();
		if (string.IsNullOrEmpty(inDir) || !Directory.Exists(inDir))
		{
			MessageBox.Show("Please select a valid Elden Ring mapbnd directory(containing mXX_XX_XX_XX_NNNNNN.mapbnd.dcx).", "Notice");
			return;
		}
		if (string.IsNullOrEmpty(outDir))
		{
			MessageBox.Show("Please select the output directory.", "Notice");
			return;
		}
		if (string.IsNullOrEmpty(mapId))
		{
			MessageBox.Show("Please enter the Nightreign target map ID(e.g. 60_43_37_00).", "Notice");
			return;
		}
		if (string.Equals(Path.GetFullPath(inDir).TrimEnd(new char[2] { '\\', '/' }), Path.GetFullPath(outDir).TrimEnd(new char[2] { '\\', '/' }), StringComparison.OrdinalIgnoreCase))
		{
			MessageBox.Show("The input and output directories cannot be the same(to avoid overwriting the original files).", "Notice");
			return;
		}
		mpBtn.Enabled = false;
		mpLog.Clear();
		try
		{
			await Task.Run(() =>
			{
				MapCore.ConvertMapPieces(inDir, outDir, mapId, (string m) =>
				{
					Log(mpLog, m);
				});
			});
			Log(mpLog, "");
			MessageBox.Show("Map tile conversion complete!\n\nOutput directory: " + outDir + "\n\nJust copy the mapbnd files inside into the Nightreign m60 map folder.", "Done");
		}
		catch (Exception ex)
		{
			Log(mpLog, "");
			Log(mpLog, "!!! Error: " + ex.Message);
			MessageBox.Show("Error:\n" + (ex.InnerException?.Message ?? ex.Message) + HintForError(ex), "Error");
		}
		finally
		{
			mpBtn.Enabled = true;
		}
	}

	private string PickOpen()
	{
		OpenFileDialog openFileDialog = new OpenFileDialog
		{
			Filter = "MSB map files (*.msb.dcx)|*.msb.dcx|All files (*.*)|*.*"
		};
		try
		{
			return (openFileDialog.ShowDialog() == DialogResult.OK) ? openFileDialog.FileName : null;
		}
		finally
		{
			((IDisposable)(object)openFileDialog)?.Dispose();
		}
	}

	private string PickSave(string current)
	{
		SaveFileDialog saveFileDialog = new SaveFileDialog
		{
			Filter = "MSB map files (*.msb.dcx)|*.msb.dcx|All files (*.*)|*.*"
		};
		try
		{
			if (!string.IsNullOrEmpty(current))
			{
				saveFileDialog.InitialDirectory = Path.GetDirectoryName(current);
				saveFileDialog.FileName = Path.GetFileName(current);
			}
			return (saveFileDialog.ShowDialog() == DialogResult.OK) ? saveFileDialog.FileName : null;
		}
		finally
		{
			((IDisposable)(object)saveFileDialog)?.Dispose();
		}
	}

	private void AutoFillConvOut()
	{
		if (!string.IsNullOrWhiteSpace(convIn.Text))
		{
			string.IsNullOrWhiteSpace(convOut.Text);
		}
		if (!string.IsNullOrWhiteSpace(convIn.Text))
		{
			try
			{
				string directoryName = Path.GetDirectoryName(convIn.Text);
				string fileName = Path.GetFileName(convIn.Text);
				string text = (fileName.EndsWith(".msb.dcx", StringComparison.OrdinalIgnoreCase) ? fileName.Substring(0, fileName.Length - 8) : Path.GetFileNameWithoutExtension(fileName));
				convOut.Text = Path.Combine(directoryName ?? "", text + "_NR.msb.dcx");
			}
			catch
			{
			}
		}
	}

	private void AutoFillMergeOut()
	{
		if (!string.IsNullOrWhiteSpace(mergeBase.Text))
		{
			try
			{
				string directoryName = Path.GetDirectoryName(mergeBase.Text);
				string fileName = Path.GetFileName(mergeBase.Text);
				string text = (fileName.EndsWith(".msb.dcx", StringComparison.OrdinalIgnoreCase) ? fileName.Substring(0, fileName.Length - 8) : Path.GetFileNameWithoutExtension(fileName));
				mergeOut.Text = Path.Combine(directoryName ?? "", text + "_Merge.msb.dcx");
			}
			catch
			{
			}
		}
	}

	private void Log(TextBox box, string msg)
	{
		if (box.InvokeRequired)
		{
			box.BeginInvoke(() =>
			{
				Log(box, msg);
			});
		}
		else
		{
			box.AppendText(msg + Environment.NewLine);
		}
	}

	private async Task RunConvert()
	{
		string inP = convIn.Text.Trim();
		string outP = convOut.Text.Trim();
		if (string.IsNullOrEmpty(inP) || !File.Exists(inP))
		{
			MessageBox.Show("Please select a valid Elden Ring map file.", "Notice");
			return;
		}
		if (string.IsNullOrEmpty(outP))
		{
			MessageBox.Show("Please specify the output location.", "Notice");
			return;
		}
		convBtn.Enabled = false;
		convLog.Clear();
		try
		{
			await Task.Run(() =>
			{
				MapCore.Convert(inP, outP, (string m) =>
				{
					Log(convLog, m);
				});
			});
			Log(convLog, "");
			MessageBox.Show("Conversion complete!\n\nOutput: " + outP, "Done");
		}
		catch (Exception ex)
		{
			Log(convLog, "");
			Log(convLog, "!!! Error: " + ex.Message);
			if (ex.InnerException != null)
			{
				Log(convLog, "    " + ex.InnerException.Message);
			}
			MessageBox.Show("Conversion error:\n" + (ex.InnerException?.Message ?? ex.Message) + HintForError(ex), "Error");
		}
		finally
		{
			convBtn.Enabled = true;
		}
	}

	private async Task RunMerge()
	{
		string baseP = mergeBase.Text.Trim();
		string addP = mergeAdd.Text.Trim();
		string outP = mergeOut.Text.Trim();
		if (string.IsNullOrEmpty(baseP) || !File.Exists(baseP))
		{
			MessageBox.Show("Please select a valid base map file.", "Notice");
			return;
		}
		if (string.IsNullOrEmpty(addP) || !File.Exists(addP))
		{
			MessageBox.Show("Please select a valid map file to add.", "Notice");
			return;
		}
		if (string.IsNullOrEmpty(outP))
		{
			MessageBox.Show("Please specify the output location.", "Notice");
			return;
		}
		mergeBtn.Enabled = false;
		mergeLog.Clear();
		string cremap = mergeCollRemap.Text.Trim();
		if (cremap.Length > 0 && !File.Exists(cremap))
		{
			MessageBox.Show("The collision remap table file does not exist(can be left empty).", "Notice");
			mergeBtn.Enabled = true;
			return;
		}
		try
		{
			await Task.Run(() =>
			{
				MapCore.Merge(baseP, addP, outP, (string m) =>
				{
					Log(mergeLog, m);
				}, (cremap.Length > 0) ? cremap : null, mergeLinkEnemyGroup.Checked);
			});
			Log(mergeLog, "");
			MessageBox.Show("Merge complete!\n\nOutput: " + outP, "Done");
		}
		catch (Exception ex)
		{
			Log(mergeLog, "");
			Log(mergeLog, "!!! Error: " + ex.Message);
			if (ex.InnerException != null)
			{
				Log(mergeLog, "    " + ex.InnerException.Message);
			}
			MessageBox.Show("Fix error:\n" + (ex.InnerException?.Message ?? ex.Message) + HintForError(ex), "Error");
		}
		finally
		{
			mergeBtn.Enabled = true;
		}
	}

	private async Task RunCopyAssets()
	{
		string mapP = assetMap.Text.Trim();
		string inDir = assetInDir.Text.Trim();
		string outDir = assetOutDir.Text.Trim();
		if (string.IsNullOrEmpty(mapP) || !File.Exists(mapP))
		{
			MessageBox.Show("Please select a valid Nightreign map file (used to read which AEGs are needed).", "Notice");
			return;
		}
		if (string.IsNullOrEmpty(inDir) || !Directory.Exists(inDir))
		{
			MessageBox.Show("Please select a valid input directory (Elden Ring unpacked directory).", "Notice");
			return;
		}
		if (string.IsNullOrEmpty(outDir))
		{
			MessageBox.Show("Please select the output directory (Nightreign mod directory).", "Notice");
			return;
		}
		if (string.Equals(Path.GetFullPath(inDir).TrimEnd(new char[2] { '\\', '/' }), Path.GetFullPath(outDir).TrimEnd(new char[2] { '\\', '/' }), StringComparison.OrdinalIgnoreCase))
		{
			MessageBox.Show("The input directory and the output directory cannot be the same.", "Notice");
			return;
		}
		int aetMode = (assetAetWhole.Checked ? 2 : (assetAetDeep.Checked ? 1 : 0));
		assetBtn.Enabled = false;
		manifestBtn.Enabled = false;
		assetLog.Clear();
		try
		{
			await Task.Run(() =>
			{
				MapCore.CopyAssetResources(mapP, inDir, outDir, aetMode, (string m) =>
				{
					Log(assetLog, m);
				});
			});
			Log(assetLog, "");
			MessageBox.Show("Copy complete!\n\nOutput directory: " + outDir, "Done");
		}
		catch (Exception ex)
		{
			Log(assetLog, "");
			Log(assetLog, "!!! Error: " + ex.Message);
			MessageBox.Show("Copy error:\n" + (ex.InnerException?.Message ?? ex.Message) + HintForError(ex), "Error");
		}
		finally
		{
			assetBtn.Enabled = true;
			manifestBtn.Enabled = true;
		}
	}

	private void RunExportManifest()
	{
		string text = assetMap.Text.Trim();
		if (string.IsNullOrEmpty(text) || !File.Exists(text))
		{
			MessageBox.Show("Please select a Nightreign map file first.", "Notice");
			return;
		}
		try
		{
			List<string> list = MapCore.CollectAegFromFile(text);
			string contents = MapCore.BuildManifestText(list, Path.GetFileName(text));
			SaveFileDialog saveFileDialog = new SaveFileDialog
			{
				Filter = "Text files (*.txt)|*.txt",
				FileName = "Required Asset list.txt"
			};
			try
			{
				if (saveFileDialog.ShowDialog() == DialogResult.OK)
				{
					File.WriteAllText(saveFileDialog.FileName, contents, new UTF8Encoding(encoderShouldEmitUTF8Identifier: true));
					Log(assetLog, $"List exported: {saveFileDialog.FileName} (total {list.Count} AEG models)");
					MessageBox.Show("List exported!\n\n" + saveFileDialog.FileName, "Done");
				}
			}
			finally
			{
				((IDisposable)(object)saveFileDialog)?.Dispose();
			}
		}
		catch (Exception ex)
		{
			Log(assetLog, "!!! Error: " + ex.Message);
			MessageBox.Show("List export error:\n" + (ex.InnerException?.Message ?? ex.Message) + HintForError(ex), "Error");
		}
	}

	private string HintForError(Exception ex)
	{
		string text = (ex.InnerException?.Message ?? ex.Message) ?? "";
		if (text.Contains("oo2core") || text.Contains("Oodle") || text.Contains("Unknown DCX"))
		{
			return "\n\nThis map may be compressed with Oodle(KRAK). Please place oo2core_9_win64.dll next to this program and try again.";
		}
		return "";
	}

	private async Task RunVbs()
	{
		string inDir = vbsInDir.Text.Trim();
		string outDir = vbsOutDir.Text.Trim();
		if (string.IsNullOrEmpty(inDir) || !Directory.Exists(inDir))
		{
			MessageBox.Show("Please select a valid input directory(containing AEG geombnd).", "Notice");
		}
		else if (string.IsNullOrEmpty(outDir))
		{
			MessageBox.Show("Please select the output directory.", "Notice");
		}
		else if (string.Equals(Path.GetFullPath(inDir).TrimEnd(new char[2] { '\\', '/' }), Path.GetFullPath(outDir).TrimEnd(new char[2] { '\\', '/' }), StringComparison.OrdinalIgnoreCase))
		{
			MessageBox.Show("The input directory and the output directory cannot be the same(to avoid overwriting the original files).", "Notice");
		}
		else
		{
			if (MessageBox.Show("The vertex buffer structure(VBS) of all AEG models in the input directory will be changed to the Nightreign native format(adaptive to the number of UV sets/vertex colors).\nVertex data(position/UV/vertex colors, etc.) will be kept and re-encoded according to the Nightreign layout.\n\nIt is recommended to test on a small number of files first, and process in batch only after confirming that they display correctly in the game.\n\nContinue?", "Confirm", MessageBoxButtons.OKCancel, MessageBoxIcon.Exclamation) != DialogResult.OK)
			{
				return;
			}
			vbsBtn.Enabled = false;
			vbsLog.Clear();
			try
			{
				await Task.Run(() =>
				{
					MapCore.BatchModifyVbs(inDir, outDir, (string m) =>
					{
						Log(vbsLog, m);
					});
				});
				Log(vbsLog, "");
				MessageBox.Show("Batch VBS change complete!\n\nOutput directory: " + outDir, "Done");
			}
			catch (Exception ex)
			{
				Log(vbsLog, "");
				Log(vbsLog, "!!! Error: " + ex.Message);
				MessageBox.Show("Fix error:\n" + (ex.InnerException?.Message ?? ex.Message) + HintForError(ex), "Error");
			}
			finally
			{
				vbsBtn.Enabled = true;
			}
		}
	}
}
