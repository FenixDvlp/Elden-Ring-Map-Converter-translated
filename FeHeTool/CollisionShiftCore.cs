using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Reflection;
using HKLib.hk2018;
using HKLib.Serialization.hk2018.Binary;
using SoulsFormats;

namespace FeHeTool;

public static class CollisionShiftCore
{
	private static Vector4 Sh3(Vector4 v, Vector4 d)
	{
		return new Vector4((MathF.Abs(v.X) > 1E+30f) ? v.X : (v.X + d.X), (MathF.Abs(v.Y) > 1E+30f) ? v.Y : (v.Y + d.Y), (MathF.Abs(v.Z) > 1E+30f) ? v.Z : (v.Z + d.Z), v.W);
	}

	private static Vector4 ShS(Vector4 v, float s)
	{
		return new Vector4((MathF.Abs(v.X) > 1E+30f) ? v.X : (v.X + s), (MathF.Abs(v.Y) > 1E+30f) ? v.Y : (v.Y + s), (MathF.Abs(v.Z) > 1E+30f) ? v.Z : (v.Z + s), (MathF.Abs(v.W) > 1E+30f) ? v.W : (v.W + s));
	}

	private static int Shift(object o, Vector4 d, HashSet<object> seen, int dep)
	{
		if (o == null || dep > 12)
		{
			return 0;
		}
		Type type = o.GetType();
		if (type.IsPrimitive || type == typeof(string) || type.IsEnum)
		{
			return 0;
		}
		if (!type.IsValueType && !seen.Add(o))
		{
			return 0;
		}
		int num = 0;
		FieldInfo[] fields = type.GetFields(BindingFlags.Instance | BindingFlags.Public);
		foreach (FieldInfo fieldInfo in fields)
		{
			object value;
			try
			{
				value = fieldInfo.GetValue(o);
			}
			catch
			{
				continue;
			}
			if (value == null)
			{
				continue;
			}
			if (value is Vector4 vector)
			{
				string name = fieldInfo.Name;
				Vector4 vector2 = vector;
				bool flag = true;
				switch (name)
				{
				case "m_min":
				case "m_max":
					vector2 = Sh3(vector, d);
					break;
				case "m_lx":
				case "m_hx":
					vector2 = ShS(vector, d.X);
					break;
				case "m_ly":
				case "m_hy":
					vector2 = ShS(vector, d.Y);
					break;
				case "m_lz":
				case "m_hz":
					vector2 = ShS(vector, d.Z);
					break;
				default:
					flag = false;
					break;
				}
				if (flag)
				{
					fieldInfo.SetValue(o, vector2);
					num++;
				}
				continue;
			}
			if (fieldInfo.Name == "m_codecParms" && value is float[] array && array.Length >= 6)
			{
				array[0] += d.X;
				array[1] += d.Y;
				array[2] += d.Z;
				num++;
				continue;
			}
			if (value is IList list)
			{
				for (int j = 0; j < list.Count; j++)
				{
					object obj2 = list[j];
					if (obj2 == null)
					{
						continue;
					}
					Type type2 = obj2.GetType();
					if (type2.IsPrimitive || type2 == typeof(string) || type2.IsEnum)
					{
						break;
					}
					if (type2.Namespace != null && type2.Namespace.StartsWith("HKLib"))
					{
						int num2 = Shift(obj2, d, seen, dep + 1);
						if (num2 > 0 && type2.IsValueType)
						{
							list[j] = obj2;
						}
						num += num2;
					}
				}
				continue;
			}
			Type type3 = value.GetType();
			if (type3.Namespace != null && type3.Namespace.StartsWith("HKLib"))
			{
				int num3 = Shift(value, d, seen, dep + 1);
				if (num3 > 0 && type3.IsValueType)
				{
					fieldInfo.SetValue(o, value);
				}
				num += num3;
			}
		}
		return num;
	}

	private static byte[] Recompress(byte[] raw)
	{
		try
		{
			return DCX.Compress(raw, DCX.Type.DCX_KRAK);
		}
		catch
		{
			return DCX.Compress(raw, DCX.Type.DCX_DFLT_11000_44_9_15);
		}
	}

	public static void ShiftBinder(string bhd, string bdt, string outDir, float dx, float dy, float dz, Action<string> log)
	{
		Vector4 d = new Vector4(dx, dy, dz, 0f);
		log($"Collision shift delta = ({dx}, {dy}, {dz})");
		BXF4 bXF = BXF4.Read(bhd, bdt);
		Directory.CreateDirectory(outDir);
		BinderFile binderFile = bXF.Files.FirstOrDefault((BinderFile f) => f.Name.ToLower().Contains("compendium"));
		if (binderFile == null)
		{
			log("Error: compendium not found in collision pack");
			return;
		}
		string path = Path.Combine(Path.GetTempPath(), "fehe.compendium");
		File.WriteAllBytes(path, DCX.Decompress(binderFile.Bytes.ToArray()).ToArray());
		HavokBinarySerializer havokBinarySerializer = new HavokBinarySerializer();
		havokBinarySerializer.LoadCompendium(path);
		string path2 = Path.Combine(Path.GetTempPath(), "fehe_in.hkx");
		string path3 = Path.Combine(Path.GetTempPath(), "fehe_out.hkx");
		int num = 0;
		int num2 = 0;
		int num3 = 0;
		foreach (BinderFile file in bXF.Files)
		{
			string text = file.Name.Split('\\', '/').Last();
			if (file.Name.ToLower().Contains("compendium"))
			{
				continue;
			}
			try
			{
				File.WriteAllBytes(path2, DCX.Decompress(file.Bytes.ToArray()).ToArray());
				if (!(havokBinarySerializer.Read(path2) is hkRootLevelContainer hkRootLevelContainer2))
				{
					log("  Skipped " + text + ": not a collision root");
					num2++;
					continue;
				}
				int num4 = Shift(hkRootLevelContainer2, d, new HashSet<object>(ReferenceEqualityComparer.Instance), 0);
				using (FileStream stream = File.Create(path3))
				{
					havokBinarySerializer.Write(hkRootLevelContainer2, stream);
				}
				file.Bytes = Recompress(File.ReadAllBytes(path3));
				num3 += num4;
				num++;
				if (num <= 3 || num % 30 == 0)
				{
					log($"  [{num}] {text}: shifted {num4} coordinate fields");
				}
			}
			catch (Exception ex)
			{
				log("  Skipped " + text + ": " + ex.Message.Split('\n')[0]);
				num2++;
			}
		}
		log($"Done: succeeded {num} blocks (total shifted {num3} coordinate fields), skipped {num2} blocks");
		if (num2 > 0)
		{
			log("Note: most skipped blocks are large multi-part collision blocks; oo2core_9_win64.dll must be placed next to the exe to decompress them.");
		}
		string text2 = Path.Combine(outDir, Path.GetFileName(bhd));
		string text3 = Path.Combine(outDir, Path.GetFileName(bdt));
		bXF.Write(text2, text3);
		log("Output: " + text2);
		log("      " + text3);
	}
}
