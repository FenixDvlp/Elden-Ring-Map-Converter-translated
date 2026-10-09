using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using SoulsFormats;

namespace FeHeTool;

public static class MapCore
{
	private const BindingFlags BF = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

	private static readonly HashSet<Type> VectorTypes = new HashSet<Type>
	{
		typeof(Vector2),
		typeof(Vector3),
		typeof(Vector4)
	};

	private static readonly Dictionary<string, string> Alias = new Dictionary<string, string>
	{
		{ "DisplayDataStruct", "DisplayData" },
		{ "DisplayGroupStruct", "DisplayGroupData" },
		{ "GparamConfigStruct", "GparamData" },
		{ "GrassConfigStruct", "GrassData" },
		{ "SceneGparamConfigStruct", "SceneGparamData" },
		{ "TileLoadConfig", "TileData" },
		{ "NPCParamID", "NpcParamId" },
		{ "ThinkParamID", "NpcThinkParamId" },
		{ "PlatoonID", "PlatoonId" },
		{ "CharaInitID", "CharaInitParamId" },
		{ "SpEffectSetParamID", "SpEffectSetParamIds" },
		{ "CollisionPartName", "HitPartName" },
		{ "WalkRouteName", "PatrolRouteName" },
		{ "LightSetID", "LightID" },
		{ "FogParamID", "FogID" }
	};

	private static readonly Regex AegRe = new Regex("^AEG(\\d+)_(\\d+)", RegexOptions.IgnoreCase);

	private const int NR_FLVER_VERSION = 131105;

	private static readonly Regex AetRefRe = new Regex("aet(\\d+)_(\\d+)", RegexOptions.IgnoreCase);

	private const int INIT_EVENT_BANK = 2000;

	private const int INIT_EVENT_ID = 0;

	private const int INIT_COMMON_BANK = 2000;

	private const int INIT_COMMON_ID = 6;

	private const int BONFIRE_ER_BANK = 2009;

	private const int BONFIRE_ER_ID = 3;

	private const int BONFIRE_NR_BANK = 2009;

	private const int BONFIRE_NR_ID = 12;

	private static bool IsDirect(Type t)
	{
		if (!t.IsPrimitive && !t.IsEnum && !(t == typeof(string)) && !(t == typeof(decimal)))
		{
			return VectorTypes.Contains(t);
		}
		return true;
	}

	private static bool IsList(Type t)
	{
		if (t.IsGenericType)
		{
			return t.GetGenericTypeDefinition() == typeof(List<>);
		}
		return false;
	}

	private static PropertyInfo FindDst(Type dstType, string srcName)
	{
		PropertyInfo property = dstType.GetProperty(srcName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
		if (property != null)
		{
			return property;
		}
		if (Alias.TryGetValue(srcName, out var value))
		{
			return dstType.GetProperty(value, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
		}
		return null;
	}

	private static object NewInst(Type t, object src)
	{
		try
		{
			ConstructorInfo constructor = t.GetConstructor(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, Type.EmptyTypes, null);
			if (constructor != null)
			{
				return constructor.Invoke(null);
			}
			ConstructorInfo constructor2 = t.GetConstructor(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, new Type[1] { typeof(string) }, null);
			if (constructor2 != null)
			{
				string text = (src?.GetType().GetProperty("Name")?.GetValue(src) as string) ?? "";
				return constructor2.Invoke(new object[1] { text });
			}
			return Activator.CreateInstance(t);
		}
		catch
		{
			return null;
		}
	}

	private static object Conv1(object v, Type tt)
	{
		if (v == null)
		{
			return null;
		}
		Type type = v.GetType();
		if (tt.IsAssignableFrom(type))
		{
			return v;
		}
		if (tt.IsEnum)
		{
			return Enum.ToObject(tt, System.Convert.ChangeType(v, Enum.GetUnderlyingType(tt)));
		}
		if (type.IsEnum && tt.IsPrimitive)
		{
			return System.Convert.ChangeType(System.Convert.ChangeType(v, Enum.GetUnderlyingType(type)), tt);
		}
		if (tt.IsPrimitive || tt == typeof(decimal))
		{
			try
			{
				return System.Convert.ChangeType(v, tt);
			}
			catch
			{
				return v;
			}
		}
		return v;
	}

	private static void Copy(object s, object d, int dep)
	{
		if (s == null || d == null || dep > 10)
		{
			return;
		}
		PropertyInfo[] properties = s.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
		foreach (PropertyInfo propertyInfo in properties)
		{
			if (!propertyInfo.CanRead || propertyInfo.GetIndexParameters().Length != 0)
			{
				continue;
			}
			PropertyInfo propertyInfo2 = FindDst(d.GetType(), propertyInfo.Name);
			if (propertyInfo2 == null)
			{
				continue;
			}
			object value;
			try
			{
				value = propertyInfo.GetValue(s);
			}
			catch
			{
				continue;
			}
			if (value == null)
			{
				continue;
			}
			Type propertyType = propertyInfo.PropertyType;
			Type propertyType2 = propertyInfo2.PropertyType;
			if (IsDirect(propertyType))
			{
				if (propertyInfo2.CanWrite)
				{
					try
					{
						propertyInfo2.SetValue(d, Conv1(value, propertyType2));
					}
					catch
					{
					}
				}
			}
			else if (propertyType.IsArray && propertyType2.IsArray)
			{
				if (!propertyInfo2.CanWrite)
				{
					continue;
				}
				try
				{
					Array array = (Array)value;
					Type elementType = propertyType2.GetElementType();
					Array array2 = Array.CreateInstance(elementType, array.Length);
					for (int j = 0; j < array.Length; j++)
					{
						array2.SetValue(Conv1(array.GetValue(j), elementType), j);
					}
					propertyInfo2.SetValue(d, array2);
				}
				catch
				{
				}
			}
			else if (IsList(propertyType) && IsList(propertyType2))
			{
				try
				{
					Type t = propertyType.GetGenericArguments()[0];
					Type type = propertyType2.GetGenericArguments()[0];
					IList list = (IList)value;
					IList list2 = (propertyInfo2.CanRead ? (propertyInfo2.GetValue(d) as IList) : null);
					if (list2 == null && propertyInfo2.CanWrite)
					{
						list2 = (IList)Activator.CreateInstance(propertyType2);
						propertyInfo2.SetValue(d, list2);
					}
					if (list2 == null)
					{
						continue;
					}
					list2.Clear();
					if (IsDirect(t))
					{
						foreach (object item in list)
						{
							list2.Add(Conv1(item, type));
						}
						continue;
					}
					foreach (object item2 in list)
					{
						object obj4 = NewInst(type, item2);
						if (obj4 != null)
						{
							Copy(item2, obj4, dep + 1);
							list2.Add(obj4);
						}
					}
				}
				catch
				{
				}
			}
			else
			{
				if (!propertyType.IsClass)
				{
					continue;
				}
				try
				{
					object obj6 = (propertyInfo2.CanRead ? propertyInfo2.GetValue(d) : null);
					if (obj6 == null && propertyInfo2.CanWrite)
					{
						obj6 = NewInst(propertyType2, value);
						if (obj6 != null)
						{
							propertyInfo2.SetValue(d, obj6);
						}
					}
					if (obj6 != null)
					{
						Copy(value, obj6, dep + 1);
					}
				}
				catch
				{
				}
			}
		}
	}

	private static void InitNull(object o, int dep)
	{
		if (o == null || dep > 10)
		{
			return;
		}
		PropertyInfo[] properties = o.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
		foreach (PropertyInfo propertyInfo in properties)
		{
			if (propertyInfo.GetIndexParameters().Length != 0)
			{
				continue;
			}
			Type propertyType = propertyInfo.PropertyType;
			if (IsDirect(propertyType) || propertyType.IsArray || IsList(propertyType) || !propertyType.IsClass)
			{
				continue;
			}
			object obj = null;
			if (propertyInfo.CanRead)
			{
				try
				{
					obj = propertyInfo.GetValue(o);
				}
				catch
				{
					continue;
				}
			}
			if (obj == null)
			{
				ConstructorInfo constructor = propertyType.GetConstructor(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, Type.EmptyTypes, null);
				if (constructor != null && propertyInfo.CanWrite)
				{
					try
					{
						obj = constructor.Invoke(null);
						propertyInfo.SetValue(o, obj);
					}
					catch
					{
						obj = null;
					}
				}
			}
			if (obj != null)
			{
				string text = obj.GetType().Namespace;
				if (text != null && text.StartsWith("SoulsFormats"))
				{
					InitNull(obj, dep + 1);
				}
			}
		}
	}

	private static void FixupPart(object src, object dst)
	{
		object obj = dst.GetType().GetProperty("EntityData", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)?.GetValue(dst);
		if (obj != null)
		{
			CopyScalar(src, "EntityID", obj, "EntityID");
			CopyArray(src, "EntityGroupIDs", obj, "EntityGroupIDs");
		}
		if (!(dst is MSB_NR.Part.Asset) || !(src.GetType().GetProperty("PartNames", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)?.GetValue(src) is string[] array))
		{
			return;
		}
		for (int i = 0; i < 6 && i < array.Length; i++)
		{
			PropertyInfo property = dst.GetType().GetProperty("PartName" + (i + 1), BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			if (property != null && property.CanWrite)
			{
				try
				{
					property.SetValue(dst, array[i]);
				}
				catch
				{
				}
			}
		}
	}

	private static void CopyScalar(object s, string sn, object d, string dn)
	{
		PropertyInfo property = s.GetType().GetProperty(sn, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
		PropertyInfo property2 = d.GetType().GetProperty(dn, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
		if (property == null || property2 == null || !property2.CanWrite)
		{
			return;
		}
		try
		{
			object value = property.GetValue(s);
			if (value != null)
			{
				property2.SetValue(d, Conv1(value, property2.PropertyType));
			}
		}
		catch
		{
		}
	}

	private static void CopyArray(object s, string sn, object d, string dn)
	{
		PropertyInfo property = s.GetType().GetProperty(sn, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
		PropertyInfo property2 = d.GetType().GetProperty(dn, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
		if (property == null || property2 == null || !property2.CanWrite || !property2.PropertyType.IsArray)
		{
			return;
		}
		try
		{
			if (property.GetValue(s) is Array array)
			{
				Type elementType = property2.PropertyType.GetElementType();
				Array array2 = Array.CreateInstance(elementType, array.Length);
				for (int i = 0; i < array.Length; i++)
				{
					array2.SetValue(Conv1(array.GetValue(i), elementType), i);
				}
				property2.SetValue(d, array2);
			}
		}
		catch
		{
		}
	}

	private static MSB_NR ConvertMsbeToNr(MSBE src, Action<string> log)
	{
		MSB_NR mSB_NR = new MSB_NR();
		CopyAll(src, mSB_NR, log);
		DropHighCollisionMsb(mSB_NR, log);
		CleanRefs(mSB_NR);
		FixCollisionTileMapID(mSB_NR, log);
		FixEnemyTileMapID(mSB_NR, log);
		mSB_NR.Compression = DCX.Type.DCX_DFLT_11000_44_9_15;
		return mSB_NR;
	}

	private static int FixCollisionTileMapID(MSB_NR map, Action<string> log)
	{
		int num = 0;
		foreach (MSB_NR.Part.Collision collision in map.Parts.Collisions)
		{
			MSB_NR.Part.TileStruct tileData = collision.TileData;
			if (tileData != null && tileData.MapID != null && tileData.MapID.Length == 4 && tileData.MapID[0] == 0 && tileData.MapID[1] == 0 && tileData.MapID[2] == 0 && tileData.MapID[3] == 0)
			{
				tileData.MapID = new sbyte[4] { -1, -1, -1, -1 };
				num++;
			}
		}
		foreach (MSB_NR.Part.ConnectCollision connectCollision in map.Parts.ConnectCollisions)
		{
			MSB_NR.Part.TileStruct tileData2 = connectCollision.TileData;
			if (tileData2 != null && tileData2.MapID != null && tileData2.MapID.Length == 4 && tileData2.MapID[0] == 0 && tileData2.MapID[1] == 0 && tileData2.MapID[2] == 0 && tileData2.MapID[3] == 0)
			{
				tileData2.MapID = new sbyte[4] { -1, -1, -1, -1 };
				num++;
			}
		}
		if (num > 0)
		{
			log?.Invoke($" Corrected collision pieces TileData.MapID [0,0,0,0]->[-1,-1,-1,-1]: {num} (required for Nightreign collision, otherwise loading crashes)");
		}
		return num;
	}

	private static int FixEnemyTileMapID(MSB_NR map, Action<string> log)
	{
		int num = 0;
		foreach (MSB_NR.Part.Enemy e in map.Parts.Enemies)
		{
			if (SetTileMapIDMinusOne(e.TileData, (MSB_NR.Part.TileStruct v) =>
			{
				e.TileData = v;
			}))
			{
				num++;
			}
		}
		if (num > 0)
		{
			log?.Invoke($" Corrected enemies TileData.MapID -> [-1,-1,-1,-1]: {num} (so that monsters spawn/display in this tile; dummy enemies not included)");
		}
		return num;
	}

	private static bool SetTileMapIDMinusOne(MSB_NR.Part.TileStruct t, Action<MSB_NR.Part.TileStruct> setBack)
	{
		if (t == null)
		{
			MSB_NR.Part.TileStruct tileStruct = new MSB_NR.Part.TileStruct();
			tileStruct.MapID = new sbyte[4] { -1, -1, -1, -1 };
			setBack(tileStruct);
			return true;
		}
		if (t.MapID == null || t.MapID.Length != 4 || t.MapID[0] != -1 || t.MapID[1] != -1 || t.MapID[2] != -1 || t.MapID[3] != -1)
		{
			t.MapID = new sbyte[4] { -1, -1, -1, -1 };
			return true;
		}
		return false;
	}

	private static bool IsHighCollisionName(string name)
	{
		if (name == null)
		{
			return false;
		}
		Match match = Regex.Match(name, "(\\d{6})");
		if (match.Success && int.TryParse(match.Groups[1].Value, out var result))
		{
			return result >= 900000;
		}
		return false;
	}

	private static int DropHighCollisionMsb(MSB_NR map, Action<string> log)
	{
		int num = map.Parts.Collisions.RemoveAll((MSB_NR.Part.Collision c) => IsHighCollisionName(c.ModelName));
		num += map.Parts.ConnectCollisions.RemoveAll((MSB_NR.Part.ConnectCollision c) => IsHighCollisionName(c.ModelName));
		int num2 = map.Models.Collisions.RemoveAll((MSB_NR.Model.Collision mm) => IsHighCollisionName(mm.Name));
		if (num > 0 || num2 > 0)
		{
			CleanRefs(map);
			log?.Invoke($" Removed 9xxxxx collisions from MSB: parts {num}, models {num2} (they reference a compendium outside the archive; keeping them would cause Nightreign/Smithbox to report errors on load)");
		}
		return num + num2;
	}

	private static MSB_NR ReadMsbAuto(string path, Action<string> log, string which)
	{
		try
		{
			return SoulsFile<MSB_NR>.Read(path);
		}
		catch (Exception ex)
		{
			try
			{
				MSBE src = SoulsFile<MSBE>.Read(path);
				log("      (" + which + "map) detected as Elden Ring MSBE format, automatically converting to Nightreign...");
				MSB_NR mSB_NR = ConvertMsbeToNr(src, log);
				log($"      ({which}map) automatic conversion complete: {mSB_NR.Parts.GetEntries().Count()} parts (map tiles{mSB_NR.Parts.MapPieces.Count} enemies{mSB_NR.Parts.Enemies.Count} assets{mSB_NR.Parts.Assets.Count})");
				return mSB_NR;
			}
			catch
			{
				throw ex;
			}
		}
	}

	public static void Convert(string inPath, string outPath, Action<string> log)
	{
		log("[1/5] Reading Elden Ring map: " + Path.GetFileName(inPath));
		MSBE mSBE = SoulsFile<MSBE>.Read(inPath);
		log($" Read successfully: {mSBE.Parts.GetEntries().Count()} parts, {mSBE.Models.GetEntries().Count()} models");
		log("[2/5] Creating Nightreign map + copying field by field (including alias mapping)...");
		MSB_NR mSB_NR = new MSB_NR();
		CopyAll(mSBE, mSB_NR, log);
		DropHighCollisionMsb(mSB_NR, log);
		log("[3/5] Cleaning up dangling references...");
		int value = CleanRefs(mSB_NR);
		log($" Cleaned up dangling references: {value} places");
		FixCollisionTileMapID(mSB_NR, log);
		FixEnemyTileMapID(mSB_NR, log);
		mSB_NR.Compression = DCX.Type.DCX_DFLT_11000_44_9_15;
		log("[4/5] Writing Nightreign map: " + Path.GetFileName(outPath));
		mSB_NR.Write(outPath);
		log("[5/5] Reading back for verification...");
		MSB_NR mSB_NR2 = SoulsFile<MSB_NR>.Read(outPath);
		log($"Done! Nightreign map: models={mSB_NR2.Models.GetEntries().Count()} parts={mSB_NR2.Parts.GetEntries().Count()} regions={mSB_NR2.Regions.GetEntries().Count()} events={mSB_NR2.Events.GetEntries().Count()}");
		log($" Part details: terrain={mSB_NR2.Parts.MapPieces.Count} enemies={mSB_NR2.Parts.Enemies.Count} assets={mSB_NR2.Parts.Assets.Count} collisions={mSB_NR2.Parts.Collisions.Count} player points={mSB_NR2.Parts.Players.Count} dummy assets={mSB_NR2.Parts.DummyAssets.Count} dummy enemies={mSB_NR2.Parts.DummyEnemies.Count} connect collisions={mSB_NR2.Parts.ConnectCollisions.Count}");
		try
		{
			List<string> list = CollectAegModels(mSB_NR2);
			string path = StripMsbDcx(outPath) + "_Required Asset list.txt";
			File.WriteAllText(path, BuildManifestText(list, Path.GetFileName(outPath)), new UTF8Encoding(encoderShouldEmitUTF8Identifier: true));
			log($" Generated required AEG/AET asset list: {Path.GetFileName(path)} (total {list.Count} AEG models)");
		}
		catch (Exception ex)
		{
			log(" (error while generating the list: " + ex.Message + ")");
		}
	}

	private static string StripMsbDcx(string path)
	{
		string? path2 = Path.GetDirectoryName(path) ?? "";
		string fileName = Path.GetFileName(path);
		string path3 = (fileName.EndsWith(".msb.dcx", StringComparison.OrdinalIgnoreCase) ? fileName.Substring(0, fileName.Length - 8) : Path.GetFileNameWithoutExtension(fileName));
		return Path.Combine(path2, path3);
	}

	private static void CopyAll(object src, object dst, Action<string> log)
	{
		PropertyInfo[] properties = src.GetType().GetProperties();
		foreach (PropertyInfo propertyInfo in properties)
		{
			object value;
			try
			{
				value = propertyInfo.GetValue(src);
			}
			catch
			{
				continue;
			}
			if (value == null || value is string || value.GetType().IsValueType)
			{
				continue;
			}
			PropertyInfo property = dst.GetType().GetProperty(propertyInfo.Name);
			if (!(property == null))
			{
				object value2;
				try
				{
					value2 = property.GetValue(dst);
				}
				catch
				{
					continue;
				}
				if (value2 != null)
				{
					CopyGroup(value, value2, propertyInfo.Name, log);
				}
			}
		}
	}

	private static void CopyGroup(object sg, object dg, string label, Action<string> log)
	{
		int num = 0;
		int num2 = 0;
		bool flag = false;
		bool flag2 = label == "Parts";
		PropertyInfo[] properties = sg.GetType().GetProperties();
		foreach (PropertyInfo propertyInfo in properties)
		{
			if (!IsList(propertyInfo.PropertyType))
			{
				continue;
			}
			IList list;
			try
			{
				list = propertyInfo.GetValue(sg) as IList;
			}
			catch
			{
				continue;
			}
			if (list == null || list.Count == 0)
			{
				continue;
			}
			flag = true;
			PropertyInfo property = dg.GetType().GetProperty(propertyInfo.Name);
			if (property == null || !IsList(property.PropertyType))
			{
				num2 += list.Count;
			}
			else
			{
				if (!(property.GetValue(dg) is IList list2))
				{
					continue;
				}
				Type t = property.PropertyType.GetGenericArguments()[0];
				int num3 = 0;
				foreach (object item in list)
				{
					if (item == null)
					{
						continue;
					}
					object obj2 = NewInst(t, item);
					if (obj2 != null)
					{
						Copy(item, obj2, 0);
						InitNull(obj2, 0);
						if (flag2)
						{
							FixupPart(item, obj2);
						}
						list2.Add(obj2);
						num3++;
					}
				}
				num += num3;
			}
		}
		if (flag)
		{
			log($"   {label}: converted {num}" + ((num2 > 0) ? $" , skipped {num2} (no corresponding subtype in Nightreign)" : ""));
		}
	}

	private static int CleanRefs(MSB_NR msb)
	{
		HashSet<string> pN = new HashSet<string>(from e in msb.Parts.GetEntries()
			select e.Name into x
			where x != null
			select x);
		HashSet<string> rN = new HashSet<string>(from e in msb.Regions.GetEntries()
			select e.Name into x
			where x != null
			select x);
		HashSet<string> eN = new HashSet<string>(from e in msb.Events.GetEntries()
			select e.Name into x
			where x != null
			select x);
		Type typeFromHandle = typeof(MSB_NR.Part);
		Type typeFromHandle2 = typeof(MSB_NR.Region);
		Type typeFromHandle3 = typeof(MSB_NR.Event);
		int num = 0;
		foreach (object item in msb.Parts.GetEntries().Cast<object>().Concat(msb.Events.GetEntries().Cast<object>())
			.Concat(msb.Regions.GetEntries().Cast<object>()))
		{
			num += CleanEntry(item, pN, rN, eN, typeFromHandle, typeFromHandle2, typeFromHandle3, 0);
		}
		return num;
	}

	private static int CleanEntry(object o, HashSet<string> pN, HashSet<string> rN, HashSet<string> eN, Type tPart, Type tRegion, Type tEvent, int dep)
	{
		if (o == null || dep > 6)
		{
			return 0;
		}
		int num = 0;
		PropertyInfo[] properties = o.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
		foreach (PropertyInfo propertyInfo in properties)
		{
			if (propertyInfo.GetIndexParameters().Length != 0 || !propertyInfo.CanRead)
			{
				continue;
			}
			MSBReference customAttribute = propertyInfo.GetCustomAttribute<MSBReference>();
			Type propertyType = propertyInfo.PropertyType;
			if (customAttribute != null)
			{
				HashSet<string> hashSet;
				if (tPart.IsAssignableFrom(customAttribute.ReferenceType))
				{
					hashSet = pN;
				}
				else if (tRegion.IsAssignableFrom(customAttribute.ReferenceType))
				{
					hashSet = rN;
				}
				else
				{
					hashSet = (tEvent.IsAssignableFrom(customAttribute.ReferenceType) ? eN : null);
				}
				if (hashSet == null)
				{
					continue;
				}
				if (propertyType == typeof(string))
				{
					string text = propertyInfo.GetValue(o) as string;
					if (!string.IsNullOrEmpty(text) && !hashSet.Contains(text) && propertyInfo.CanWrite)
					{
						try
						{
							propertyInfo.SetValue(o, null);
							num++;
						}
						catch
						{
						}
					}
				}
				else
				{
					if (!(propertyType == typeof(string[])) || !(propertyInfo.GetValue(o) is string[] array))
					{
						continue;
					}
					for (int j = 0; j < array.Length; j++)
					{
						if (!string.IsNullOrEmpty(array[j]) && !hashSet.Contains(array[j]))
						{
							array[j] = null;
							num++;
						}
					}
				}
			}
			else
			{
				if (!propertyType.IsClass || !(propertyType != typeof(string)) || propertyType.IsArray || IsList(propertyType))
				{
					continue;
				}
				object obj2 = null;
				try
				{
					obj2 = propertyInfo.GetValue(o);
				}
				catch
				{
				}
				if (obj2 != null)
				{
					string text2 = obj2.GetType().Namespace;
					if (text2 != null && text2.StartsWith("SoulsFormats"))
					{
						num += CleanEntry(obj2, pN, rN, eN, tPart, tRegion, tEvent, dep + 1);
					}
				}
			}
		}
		return num;
	}

	public static void FixMsbCollisionMapID(string inPath, string outPath, Action<string> log)
	{
		log("[1/3] Reading MSB: " + Path.GetFileName(inPath));
		MSB_NR mSB_NR = SoulsFile<MSB_NR>.Read(inPath);
		DCX.Type compression = mSB_NR.Compression;
		log($" Collision parts {mSB_NR.Parts.Collisions.Count}, connect collisions {mSB_NR.Parts.ConnectCollisions.Count} (compression {compression})");
		log("[2/3] Correcting collision pieces TileData.MapID + removing 9xxxxx collisions...");
		int num = DropHighCollisionMsb(mSB_NR, log);
		int num2 = FixCollisionTileMapID(mSB_NR, log);
		if (num2 == 0 && num == 0)
		{
			log(" Nothing to process (MapID is already correct, and there are no 9xxxxx collisions)");
		}
		mSB_NR.Compression = compression;
		log("[3/3] Writing: " + Path.GetFileName(outPath));
		mSB_NR.Write(outPath);
		MSB_NR mSB_NR2 = SoulsFile<MSB_NR>.Read(outPath);
		int num3 = 0;
		foreach (MSB_NR.Part.Collision collision in mSB_NR2.Parts.Collisions)
		{
			sbyte[] mapID = collision.TileData.MapID;
			if (mapID != null && mapID.Length == 4 && mapID[0] == 0 && mapID[1] == 0 && mapID[2] == 0 && mapID[3] == 0)
			{
				num3++;
			}
		}
		log($"Done! Corrected a total of {num2} collision pieces; collision pieces still [0,0,0,0] after read-back verification = {num3} (should be 0)");
	}

	public static void FixMsbEnemyMapID(string inPath, string outPath, Action<string> log)
	{
		log("[1/3] Reading MSB: " + Path.GetFileName(inPath));
		MSB_NR mSB_NR = SoulsFile<MSB_NR>.Read(inPath);
		DCX.Type compression = mSB_NR.Compression;
		log($" Real enemies {mSB_NR.Parts.Enemies.Count} (compression {compression}; dummy enemies DummyEnemy {mSB_NR.Parts.DummyEnemies.Count} not processed)");
		log("[2/3] Setting TileData.MapID of all real enemies to [-1,-1,-1,-1]...");
		int num = FixEnemyTileMapID(mSB_NR, log);
		if (num == 0)
		{
			log(" Nothing to process (all enemies' MapID is already -1)");
		}
		mSB_NR.Compression = compression;
		log("[3/3] Writing: " + Path.GetFileName(outPath));
		mSB_NR.Write(outPath);
		MSB_NR mSB_NR2 = SoulsFile<MSB_NR>.Read(outPath);
		int num2 = 0;
		foreach (MSB_NR.Part.Enemy enemy in mSB_NR2.Parts.Enemies)
		{
			sbyte[] array = enemy.TileData?.MapID;
			if (array == null || array.Length != 4 || array[0] != -1 || array[1] != -1 || array[2] != -1 || array[3] != -1)
			{
				num2++;
			}
		}
		log($"Done! Corrected a total of {num} enemies; enemies whose MapID is not [-1,-1,-1,-1] after read-back verification = {num2} (should be 0)");
	}

	public static void Merge(string basePath, string addPath, string outPath, Action<string> log, string collisionRemapPath = null, bool linkEnemyGroup = true)
	{
		log("[1/5] Reading base map: " + Path.GetFileName(basePath));
		MSB_NR mSB_NR = ReadMsbAuto(basePath, log, "base");
		DCX.Type compression = mSB_NR.Compression;
		log($" Base map: parts={mSB_NR.Parts.GetEntries().Count()} models={mSB_NR.Models.GetEntries().Count()} (compression {compression})");
		log("[2/5] Reading map to add: " + Path.GetFileName(addPath));
		MSB_NR mSB_NR2 = ReadMsbAuto(addPath, log, "added");
		log($" Added map: parts={mSB_NR2.Parts.GetEntries().Count()} models={mSB_NR2.Models.GetEntries().Count()}");
		if (!string.IsNullOrEmpty(collisionRemapPath) && File.Exists(collisionRemapPath))
		{
			Dictionary<string, string> dictionary = LoadNameRemap(collisionRemapPath);
			int value = ApplyCollisionModelRemap(mSB_NR2, dictionary);
			log($" Applying collision remap table ({dictionary.Count} entries): changed {value} collision model/part references in the added map");
		}
		log("[3/5] Handling name conflicts (base map stays unchanged, conflicting entries of the added map are renamed)...");
		HashSet<string> existing = new HashSet<string>(from e in mSB_NR.Parts.GetEntries()
			select e.Name);
		Dictionary<string, string> dictionary2 = BuildRename(mSB_NR2.Parts.GetEntries(), existing);
		HashSet<string> existing2 = new HashSet<string>(from e in mSB_NR.Regions.GetEntries()
			select e.Name);
		Dictionary<string, string> dictionary3 = BuildRename(mSB_NR2.Regions.GetEntries(), existing2);
		HashSet<string> existing3 = new HashSet<string>(from e in mSB_NR.Events.GetEntries()
			select e.Name);
		Dictionary<string, string> dictionary4 = BuildRename(mSB_NR2.Events.GetEntries(), existing3);
		log($" Renamed: parts {dictionary2.Count}, regions {dictionary3.Count}, events {dictionary4.Count}");
		ApplyRename(mSB_NR2.Parts.GetEntries(), dictionary2);
		ApplyRename(mSB_NR2.Regions.GetEntries(), dictionary3);
		ApplyRename(mSB_NR2.Events.GetEntries(), dictionary4);
		if (dictionary2.Count > 0)
		{
			foreach (object item in mSB_NR2.Parts.GetEntries().Cast<object>().Concat(mSB_NR2.Events.GetEntries().Cast<object>())
				.Concat(mSB_NR2.Regions.GetEntries().Cast<object>()))
			{
				RemapRefs(item, dictionary2, 0);
			}
		}
		if (linkEnemyGroup)
		{
			LinkEnemiesToMapEnemyGroup(mSB_NR, mSB_NR2, log);
		}
		log("[4/5] Adding all components (models deduplicated, everything else kept)...");
		int value2 = AppendGroup(mSB_NR.Models, mSB_NR2.Models, dedupByName: true);
		int value3 = AppendGroup(mSB_NR.Parts, mSB_NR2.Parts, dedupByName: false);
		int value4 = AppendGroup(mSB_NR.Regions, mSB_NR2.Regions, dedupByName: false);
		int value5 = AppendGroup(mSB_NR.Events, mSB_NR2.Events, dedupByName: false);
		int value6 = AppendGroup(mSB_NR.Routes, mSB_NR2.Routes, dedupByName: false);
		log($" Added: models+{value2} parts+{value3} regions+{value4} events+{value5} routes+{value6}");
		DropHighCollisionMsb(mSB_NR, log);
		FixCollisionTileMapID(mSB_NR, log);
		FixEnemyTileMapID(mSB_NR, log);
		mSB_NR.Compression = compression;
		log("[5/5] Writing merged map: " + Path.GetFileName(outPath));
		mSB_NR.Write(outPath);
		MSB_NR mSB_NR3 = SoulsFile<MSB_NR>.Read(outPath);
		log($"Done! Merged map: models={mSB_NR3.Models.GetEntries().Count()} parts={mSB_NR3.Parts.GetEntries().Count()} regions={mSB_NR3.Regions.GetEntries().Count()} events={mSB_NR3.Events.GetEntries().Count()}");
	}

	private static void LinkEnemiesToMapEnemyGroup(MSB_NR baseMap, MSB_NR add, Action<string> log)
	{
		if (add.Parts.Enemies.Count == 0)
		{
			return;
		}
		Dictionary<uint, int> dictionary = new Dictionary<uint, int>();
		foreach (MSB_NR.Part.Enemy enemy in baseMap.Parts.Enemies)
		{
			uint[] entityGroupIDs = enemy.EntityData.EntityGroupIDs;
			foreach (uint num in entityGroupIDs)
			{
				if (num != 0)
				{
					dictionary.TryGetValue(num, out var value);
					dictionary[num] = value + 1;
				}
			}
		}
		if (dictionary.Count == 0)
		{
			log(" [Enemy group] The enemies of the base map have no EntityGroup, so the map enemy group cannot be detected automatically -> skipped (ported enemies may not spawn, needs manual handling)");
			return;
		}
		uint key = dictionary.OrderByDescending((KeyValuePair<uint, int> kv) => kv.Value).First().Key;
		int value2 = dictionary[key];
		log($" [Enemy group] Detected map enemy group = {key} (in base, {value2}/{baseMap.Parts.Enemies.Count} enemies are using it)");
		int num2 = 0;
		int num3 = 0;
		int num4 = 0;
		foreach (MSB_NR.Part.Enemy enemy2 in add.Parts.Enemies)
		{
			uint[] entityGroupIDs2 = enemy2.EntityData.EntityGroupIDs;
			if (Enumerable.Contains(entityGroupIDs2, key))
			{
				num3++;
				continue;
			}
			int num5 = Array.IndexOf(entityGroupIDs2, 0u);
			if (num5 < 0)
			{
				num4++;
				continue;
			}
			entityGroupIDs2[num5] = key;
			num2++;
		}
		log($" [Enemy group] Added {num2} ported enemies to group {key}" + ((num3 > 0) ? $", {num3}, already in the group: " : "") + ((num4 > 0) ? $", {num4}, no free slot (all 8 groups full), could not be added: " : ""));
		log(" [Enemy group] => When the map loads, the common event spawns monsters by this group, so the ported enemies should now display. If they still do not spawn, check whether the models/NpcParam are complete.");
	}

	private static Dictionary<string, string> BuildRename(IEnumerable<IMsbEntry> addEntries, HashSet<string> existing)
	{
		Dictionary<string, string> dictionary = new Dictionary<string, string>();
		HashSet<string> hashSet = new HashSet<string>(existing);
		foreach (IMsbEntry addEntry in addEntries)
		{
			hashSet.Add(addEntry.Name);
		}
		foreach (IMsbEntry addEntry2 in addEntries)
		{
			if (existing.Contains(addEntry2.Name) && !dictionary.ContainsKey(addEntry2.Name))
			{
				string name = addEntry2.Name;
				int num = 2;
				string text;
				do
				{
					text = name + "_" + num;
					num++;
				}
				while (hashSet.Contains(text));
				dictionary[addEntry2.Name] = text;
				hashSet.Add(text);
			}
		}
		return dictionary;
	}

	private static void ApplyRename(IEnumerable<IMsbEntry> entries, Dictionary<string, string> map)
	{
		foreach (IMsbEntry entry in entries)
		{
			if (map.TryGetValue(entry.Name, out var value))
			{
				entry.Name = value;
			}
		}
	}

	private static void RemapRefs(object o, Dictionary<string, string> partMap, int dep)
	{
		if (o == null || dep > 6 || partMap.Count == 0)
		{
			return;
		}
		Type typeFromHandle = typeof(MSB_NR.Part);
		PropertyInfo[] properties = o.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
		foreach (PropertyInfo propertyInfo in properties)
		{
			if (propertyInfo.GetIndexParameters().Length != 0 || !propertyInfo.CanRead)
			{
				continue;
			}
			MSBReference customAttribute = propertyInfo.GetCustomAttribute<MSBReference>();
			Type propertyType = propertyInfo.PropertyType;
			if (customAttribute != null && typeFromHandle.IsAssignableFrom(customAttribute.ReferenceType))
			{
				if (propertyType == typeof(string))
				{
					if (propertyInfo.GetValue(o) is string key && partMap.TryGetValue(key, out var value) && propertyInfo.CanWrite)
					{
						try
						{
							propertyInfo.SetValue(o, value);
						}
						catch
						{
						}
					}
				}
				else
				{
					if (!(propertyType == typeof(string[])) || !(propertyInfo.GetValue(o) is string[] array))
					{
						continue;
					}
					for (int j = 0; j < array.Length; j++)
					{
						if (array[j] != null && partMap.TryGetValue(array[j], out var value2))
						{
							array[j] = value2;
						}
					}
				}
			}
			else
			{
				if (!propertyType.IsClass || !(propertyType != typeof(string)) || propertyType.IsArray || IsList(propertyType))
				{
					continue;
				}
				object obj2 = null;
				try
				{
					obj2 = propertyInfo.GetValue(o);
				}
				catch
				{
				}
				if (obj2 != null)
				{
					string text = obj2.GetType().Namespace;
					if (text != null && text.StartsWith("SoulsFormats"))
					{
						RemapRefs(obj2, partMap, dep + 1);
					}
				}
			}
		}
	}

	private static int AppendGroup(object baseGroup, object addGroup, bool dedupByName)
	{
		int num = 0;
		PropertyInfo[] properties = addGroup.GetType().GetProperties();
		foreach (PropertyInfo propertyInfo in properties)
		{
			if (!IsList(propertyInfo.PropertyType) || !(propertyInfo.GetValue(addGroup) is IList { Count: not 0 } list))
			{
				continue;
			}
			PropertyInfo property = baseGroup.GetType().GetProperty(propertyInfo.Name);
			if (property == null || !IsList(property.PropertyType) || !(property.GetValue(baseGroup) is IList list2))
			{
				continue;
			}
			HashSet<string> hashSet = (dedupByName ? new HashSet<string>(from IMsbEntry e in list2
				select e.Name) : null);
			foreach (object item in list)
			{
				if (!dedupByName || !(item is IMsbEntry msbEntry) || !hashSet.Contains(msbEntry.Name))
				{
					list2.Add(item);
					num++;
				}
			}
		}
		return num;
	}

	public static List<string> CollectAegModels(object map)
	{
		HashSet<string> hashSet = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
		PropertyInfo property = map.GetType().GetProperty("Models");
		if (property != null)
		{
			object value = property.GetValue(map);
			MethodInfo method = value.GetType().GetMethod("GetEntries");
			if (method != null)
			{
				foreach (object item in (IEnumerable)method.Invoke(value, null))
				{
					if (item.GetType().GetProperty("Name")?.GetValue(item) is string text && AegRe.IsMatch(text))
					{
						hashSet.Add(NormAeg(text));
					}
				}
			}
		}
		PropertyInfo property2 = map.GetType().GetProperty("Parts");
		if (property2 != null)
		{
			object value2 = property2.GetValue(map);
			MethodInfo method2 = value2.GetType().GetMethod("GetEntries");
			if (method2 != null)
			{
				foreach (object item2 in (IEnumerable)method2.Invoke(value2, null))
				{
					if (item2.GetType().GetProperty("ModelName")?.GetValue(item2) is string text2 && AegRe.IsMatch(text2))
					{
						hashSet.Add(NormAeg(text2));
					}
				}
			}
		}
		List<string> list = hashSet.ToList();
		list.Sort(StringComparer.OrdinalIgnoreCase);
		return list;
	}

	private static string NormAeg(string name)
	{
		Match match = AegRe.Match(name);
		return "AEG" + match.Groups[1].Value + "_" + match.Groups[2].Value;
	}

	private static (string group, string id) SplitAeg(string aegName)
	{
		Match match = AegRe.Match(aegName);
		return (group: match.Groups[1].Value, id: match.Groups[2].Value);
	}

	public static List<string> CollectAegFromFile(string mapPath)
	{
		object map;
		try
		{
			map = SoulsFile<MSB_NR>.Read(mapPath);
		}
		catch
		{
			map = SoulsFile<MSBE>.Read(mapPath);
		}
		return CollectAegModels(map);
	}

	public static string BuildManifestText(List<string> aegNames, string mapName)
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.AppendLine("============================================================");
		stringBuilder.AppendLine(" Required AEG (model) / AET (texture) asset list");
		if (!string.IsNullOrEmpty(mapName))
		{
			stringBuilder.AppendLine(" Source map: " + mapName);
		}
		stringBuilder.AppendLine("============================================================");
		stringBuilder.AppendLine();
		stringBuilder.AppendLine("Note: copy these files from Elden Ring (after unpacking) to the corresponding location in the Nightreign mod,");
		stringBuilder.AppendLine(" so that the buildings/assets can be displayed (the \"Copy model assets\" tab of this tool can do this automatically).");
		stringBuilder.AppendLine(" AEG = model geometry + collision (per model), AET = textures.");
		stringBuilder.AppendLine(" Each AEG model = aegGGG_III.geombnd (model) + aegGGG_III_h/_l.geomhkxbnd (high/low collision).");
		stringBuilder.AppendLine(" AET defaults to [Exact]: for each building, only the textures of its matching number aetGGG_III are copied (least redundancy).");
		stringBuilder.AppendLine(" If some buildings appear white/purple in the game (missing shared textures), switch to the \"Whole group\" mode in the tool and copy the entire aetGGG package.");
		stringBuilder.AppendLine();
		List<string> list = (from x in aegNames.Select((string n) => SplitAeg(n).@group).Distinct()
			orderby x
			select x).ToList();
		StringBuilder stringBuilder2 = stringBuilder;
		StringBuilder stringBuilder3 = stringBuilder2;
		StringBuilder.AppendInterpolatedStringHandler handler = new StringBuilder.AppendInterpolatedStringHandler(26, 1, stringBuilder2);
		handler.AppendLiteral("── AEG model folders to copy (");
		handler.AppendFormatted(list.Count);
		handler.AppendLiteral(") ──");
		stringBuilder3.AppendLine(ref handler);
		stringBuilder.AppendLine("  " + string.Join("  ", list.Select((string g) => "asset/aeg/aeg" + g)));
		stringBuilder.AppendLine();
		stringBuilder2 = stringBuilder;
		StringBuilder stringBuilder4 = stringBuilder2;
		handler = new StringBuilder.AppendInterpolatedStringHandler(42, 1, stringBuilder2);
		handler.AppendLiteral("── AET texture groups involved (");
		handler.AppendFormatted(list.Count);
		handler.AppendLiteral("; exact mode only takes the textures matching each building's number within each group) ──");
		stringBuilder4.AppendLine(ref handler);
		stringBuilder.AppendLine("  " + string.Join("  ", list.Select((string g) => "asset/aet/aet" + g)));
		stringBuilder.AppendLine();
		stringBuilder2 = stringBuilder;
		StringBuilder stringBuilder5 = stringBuilder2;
		handler = new StringBuilder.AppendInterpolatedStringHandler(26, 1, stringBuilder2);
		handler.AppendLiteral("── Itemized list of required AEG models (total ");
		handler.AppendFormatted(aegNames.Count);
		handler.AppendLiteral(") ──");
		stringBuilder5.AppendLine(ref handler);
		stringBuilder.AppendLine(" (these are the specific model file name prefixes to copy from asset/aeg/aegGGG/;");
		stringBuilder.AppendLine(" the matching textures are in their respective asset/aet/aetGGG/; exact mode takes the same-number aetGGG_III, whole-group mode takes the entire aetGGG)");
		string text = null;
		foreach (string aegName in aegNames)
		{
			string item = SplitAeg(aegName).group;
			if (item != text)
			{
				text = item;
				stringBuilder2 = stringBuilder;
				StringBuilder stringBuilder6 = stringBuilder2;
				handler = new StringBuilder.AppendInterpolatedStringHandler(7, 1, stringBuilder2);
				handler.AppendLiteral("  [aeg");
				handler.AppendFormatted(item);
				handler.AppendLiteral("]");
				stringBuilder6.AppendLine(ref handler);
			}
			stringBuilder2 = stringBuilder;
			StringBuilder stringBuilder7 = stringBuilder2;
			handler = new StringBuilder.AppendInterpolatedStringHandler(5, 1, stringBuilder2);
			handler.AppendLiteral("     ");
			handler.AppendFormatted(aegName);
			stringBuilder7.AppendLine(ref handler);
		}
		return stringBuilder.ToString();
	}

	public static void CopyAssetResources(string mapPath, string inputDir, string outputDir, int aetMode, Action<string> log)
	{
		string[] array = new string[3] { "Exact (copy only the textures of the matching buildings)", "Deep exact (read the material database to fill in shared textures)", "Whole group copy (safest / lots of redundancy)" };
		if (aetMode < 0 || aetMode > 2)
		{
			aetMode = 0;
		}
		log("[1/6] Reading map, collecting required AEG models: " + Path.GetFileName(mapPath));
		List<string> list = CollectAegFromFile(mapPath);
		HashSet<string> hashSet = list.Select((string n) => SplitAeg(n).group).Distinct().ToHashSet();
		log($" Need {list.Count} AEG models, involving {hashSet.Count} groups");
		log(" AET strategy: " + array[aetMode]);
		if (list.Count == 0)
		{
			log(" This map has no AEG asset models, nothing to copy.");
			return;
		}
		HashSet<string> hashSet2 = new HashSet<string>(list.Select((string n) => n.ToLowerInvariant()));
		HashSet<string> source = new HashSet<string>(hashSet.Select((string g) => ("aet" + g).ToLowerInvariant()));
		log("[2/6] Recursively traversing all subfolders of the input directory...");
		List<string> list2 = new List<string>();
		int dirCount = 0;
		WalkDir(inputDir, list2, ref dirCount, log);
		log($" Scan complete: {dirCount} folders, {list2.Count} files");
		log("[3/6] Matching AEG models + collision files...");
		List<string> list3 = new List<string>();
		List<string> list4 = new List<string>();
		HashSet<string> foundModel = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
		HashSet<string> foundCol = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
		Dictionary<string, string> dictionary = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
		foreach (string item in list2)
		{
			string text = Path.GetFileName(item).ToLowerInvariant();
			if (!text.StartsWith("aeg"))
			{
				continue;
			}
			foreach (string item2 in hashSet2)
			{
				if (!StartsWithToken(text, item2))
				{
					continue;
				}
				if (text.Contains(".geomhkxbnd"))
				{
					list4.Add(item);
					foundCol.Add(item2);
				}
				else if (text.Contains(".geombnd"))
				{
					list3.Add(item);
					foundModel.Add(item2);
					if (!dictionary.ContainsKey(item2))
					{
						dictionary[item2] = item;
					}
				}
				else
				{
					list3.Add(item);
					foundModel.Add(item2);
				}
				break;
			}
		}
		log($" Models(geombnd): {list3.Count} files / covering {foundModel.Count} models");
		log($" Collision(geomhkxbnd h+l): {list4.Count} files / covering {foundCol.Count} models");
		HashSet<string> hashSet3 = null;
		if (aetMode != 2)
		{
			hashSet3 = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
			foreach (string item3 in list)
			{
				var (text2, text3) = SplitAeg(item3);
				hashSet3.Add(("aet" + text2 + "_" + text3).ToLowerInvariant());
			}
			log($"[4/6] Exact AET(same number): {hashSet3.Count} matching texture prefixes");
			if (aetMode == 1)
			{
				log(" Deep mode: reading the model material database to fill in shared textures(requires oo2core, slower)...");
				HashSet<string> hashSet4 = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
				int num = 0;
				bool oodleFail = false;
				int probed = 0;
				foreach (KeyValuePair<string, string> item4 in dictionary)
				{
					(HashSet<string>, HashSet<string>) tuple2 = ExtractMatInfoFromGeombnd(item4.Value, ref oodleFail, ref probed);
					if (oodleFail)
					{
						break;
					}
					if (tuple2.Item1.Count > 0 || tuple2.Item2.Count > 0)
					{
						num++;
					}
					foreach (string item5 in tuple2.Item1)
					{
						hashSet4.Add(item5);
					}
					foreach (string item6 in tuple2.Item2)
					{
						hashSet3.Add(item6);
					}
				}
				if (oodleFail)
				{
					log(" ⚠ Cannot read geombnd(oo2core_9_win64.dll missing). Deep completion skipped, still copying exactly by same number. Place that dll next to the exe to enable deep mode.");
				}
				else
				{
					log($" Read {num} model materials, collected {hashSet4.Count} material(matbin) names");
					string text4 = list2.FirstOrDefault((string f) =>
					{
						string text5 = Path.GetFileName(f).ToLowerInvariant();
						return text5.StartsWith("allmaterial") && text5.Contains(".matbinbnd");
					});
					if (text4 != null && hashSet4.Count > 0)
					{
						log(" Reading " + Path.GetFileName(text4) + " to parse texture references...");
						foreach (string item7 in CollectAetFromMatbins(text4, hashSet4, log))
						{
							hashSet3.Add(item7);
						}
					}
					else if (text4 == null)
					{
						log(" (allmaterial.matbinbnd.dcx not found in the input directory; skipping shared texture completion, same-number exact still applies)");
					}
					log($" Deep exact total: {hashSet3.Count} texture prefixes");
				}
			}
		}
		else
		{
			log("[4/6] (Whole group mode: copying the entire aetGGG folders in use)");
		}
		log("[5/6] Copying AEG models + collision...");
		int copied = 0;
		int copied2 = 0;
		int copied3 = 0;
		int skipped = 0;
		int failed = 0;
		foreach (string item8 in list3)
		{
			string fnl = Path.GetFileName(item8).ToLowerInvariant();
			CopyOne(item8, inputDir, outputDir, isAeg: true, fnl, ref copied, ref skipped, ref failed, log);
		}
		foreach (string item9 in list4)
		{
			string fnl2 = Path.GetFileName(item9).ToLowerInvariant();
			CopyOne(item9, inputDir, outputDir, isAeg: true, fnl2, ref copied2, ref skipped, ref failed, log);
		}
		log("[6/6] Copying AET textures...");
		foreach (string item10 in list2)
		{
			string fnl3 = Path.GetFileName(item10).ToLowerInvariant();
			if (fnl3.StartsWith("aet") && ((aetMode == 2) ? source.Any((string pre) => StartsWithToken(fnl3, pre)) : hashSet3.Any((string stem) => StartsWithToken(fnl3, stem))))
			{
				CopyOne(item10, inputDir, outputDir, isAeg: false, fnl3, ref copied3, ref skipped, ref failed, log);
			}
		}
		log("Done.");
		log($" Copied: models(geombnd) {copied}, collision(geomhkxbnd) {copied2}, AET textures {copied3}" + ((skipped > 0) ? $", skipped existing {skipped}" : "") + ((failed > 0) ? $", failed {failed}" : ""));
		List<string> list5 = foundModel.Where((string m) => !foundCol.Contains(m)).ToList();
		if (list4.Count > 0 && copied2 == 0 && skipped == 0)
		{
			log(" ⚠ Collision files were matched but none were copied successfully, check the output directory permissions.");
		}
		else if (list4.Count == 0)
		{
			log(" ⚠ Not a single collision file(geomhkxbnd) was matched! Make sure the input directory contains aegXXX_YYY_h/_l.geomhkxbnd.dcx (they are in the same aeg folder as the models).");
		}
		else if (list5.Count > 0)
		{
			log($" Note: {list5.Count} models have no collision files(many small assets have no collision to begin with, which is normal); all those with collision were copied too.");
		}
		List<string> list6 = list.Where((string n) => !foundModel.Contains(n.ToLowerInvariant())).ToList();
		if (list6.Count > 0)
		{
			log($" ⚠ {list6.Count}/{list.Count} AEG models had no geombnd found in the input directory (Elden Ring did not unpack these assets / they are not in this directory, so these buildings will be missing in the game):");
			log("        " + string.Join(", ", list6.Take(40)) + ((list6.Count > 40) ? $" ...(total {list6.Count})" : ""));
		}
		else
		{
			log(" ✓ The geombnd of all required AEG models were found and copied.");
		}
		if (aetMode != 2)
		{
			log(" Note: AET is in exact mode, only the textures of the matching buildings were copied. If some buildings appear white/purple in the game(missing textures), select the \"Whole group\" mode and copy again.");
		}
	}

	private static void CopyOne(string f, string inputDir, string outputDir, bool isAeg, string fnl, ref int copied, ref int skipped, ref int failed, Action<string> log)
	{
		string path = MakeRelative(f, inputDir, isAeg, fnl);
		string text = Path.Combine(outputDir, path);
		try
		{
			Directory.CreateDirectory(Path.GetDirectoryName(text));
			FileInfo fileInfo = new FileInfo(f);
			if (File.Exists(text) && new FileInfo(text).Length == fileInfo.Length)
			{
				skipped++;
				return;
			}
			File.Copy(f, text, overwrite: true);
			copied++;
		}
		catch (Exception ex)
		{
			failed++;
			log("      copy failed: " + Path.GetFileName(f) + " (" + ex.Message + ")");
		}
	}

	private static bool StartsWithToken(string fileNameLower, string token)
	{
		if (!fileNameLower.StartsWith(token))
		{
			return false;
		}
		if (fileNameLower.Length == token.Length)
		{
			return true;
		}
		char c = fileNameLower[token.Length];
		if (c != '.')
		{
			return c == '_';
		}
		return true;
	}

	private static string MakeRelative(string fullPath, string inputDir, bool isAeg, string fileNameLower)
	{
		string text = fullPath.Replace('\\', '/');
		int num = text.ToLowerInvariant().IndexOf("/asset/");
		if (num >= 0)
		{
			return text.Substring(num + 1).Replace('/', Path.DirectorySeparatorChar);
		}
		if (text.ToLowerInvariant().StartsWith("asset/"))
		{
			return text.Replace('/', Path.DirectorySeparatorChar);
		}
		Match match = Regex.Match(fileNameLower, "^(aeg|aet)(\\d+)");
		string fileName = Path.GetFileName(fullPath);
		if (match.Success)
		{
			string value = match.Groups[1].Value;
			string value2 = match.Groups[2].Value;
			return Path.Combine("asset", value, value + value2, fileName);
		}
		return fileName;
	}

	private static void WalkDir(string dir, List<string> files, ref int dirCount, Action<string> log)
	{
		dirCount++;
		try
		{
			foreach (string item in Directory.EnumerateFiles(dir))
			{
				files.Add(item);
			}
		}
		catch
		{
		}
		string[] array = Array.Empty<string>();
		try
		{
			array = Directory.GetDirectories(dir);
		}
		catch
		{
		}
		string[] array2 = array;
		for (int i = 0; i < array2.Length; i++)
		{
			WalkDir(array2[i], files, ref dirCount, log);
		}
	}

	private static void ApplyVbsToFlver(FLVER2 flver, bool dropColor = false)
	{
		flver.Header.Version = 131105;
		flver.BufferLayouts.Clear();
		Dictionary<string, int> layoutCache = new Dictionary<string, int>();
		foreach (FLVER2.Mesh mesh in flver.Meshes)
		{
			if (mesh.Vertices.Count == 0)
			{
				mesh.VertexBuffers.Clear();
				continue;
			}
			int num = mesh.Vertices.Max((FLVER.Vertex v) => v.UVs.Count);
			bool flag = !dropColor && mesh.Vertices.Max((FLVER.Vertex v) => v.Colors.Count) > 0;
			List<(int, FLVER.LayoutType, FLVER.LayoutSemantic, int)> list = new List<(int, FLVER.LayoutType, FLVER.LayoutSemantic, int)>
			{
				(0, FLVER.LayoutType.Float3, FLVER.LayoutSemantic.Position, 0),
				(1, FLVER.LayoutType.UByte4, FLVER.LayoutSemantic.Normal, 0),
				(2, FLVER.LayoutType.UByte4, FLVER.LayoutSemantic.Tangent, 0),
				(4, FLVER.LayoutType.UShort2, FLVER.LayoutSemantic.BoneIndices, 0)
			};
			int num2 = 7;
			if (flag)
			{
				list.Add((num2, FLVER.LayoutType.UByte4Norm, FLVER.LayoutSemantic.VertexColor, 0));
				num2++;
			}
			list.Add((num2, FLVER.LayoutType.Short2, FLVER.LayoutSemantic.UV, 0));
			num2++;
			int num3 = 1;
			int num4 = (Math.Max(0, num - 1) + 1) / 2;
			for (int num5 = 0; num5 < num4; num5++)
			{
				list.Add((num2, FLVER.LayoutType.UByte4Norm, FLVER.LayoutSemantic.UV, num3));
				num2++;
				num3++;
			}
			int num6 = 1 + num4 * 2;
			int num7 = 1;
			int num8 = (flag ? 1 : 0);
			foreach (FLVER.Vertex vertex in mesh.Vertices)
			{
				while (vertex.UVs.Count < num6)
				{
					vertex.UVs.Add((vertex.UVs.Count > 0) ? vertex.UVs[vertex.UVs.Count - 1] : Vector3.Zero);
				}
				while (vertex.Tangents.Count < num7)
				{
					vertex.Tangents.Add((vertex.Tangents.Count > 0) ? vertex.Tangents[vertex.Tangents.Count - 1] : new Vector4(1f, 0f, 0f, 1f));
				}
				while (vertex.Colors.Count < num8)
				{
					vertex.Colors.Add(new FLVER.VertexColor(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue));
				}
			}
			mesh.VertexBuffers.Clear();
			foreach (var item2 in list)
			{
				int layoutIndex = GetLayout(item2.Item2, item2.Item3, item2.Item4, item2.Item1);
				mesh.VertexBuffers.Add(new FLVER2.VertexBuffer(layoutIndex)
				{
					BufferIndex = item2.Item1,
					EdgeCompressed = false
				});
			}
		}
		int GetLayout(FLVER.LayoutType type, FLVER.LayoutSemantic sem, int idx, int stream)
		{
			string key = $"{(int)type}_{(int)sem}_{idx}_{stream}";
			if (layoutCache.TryGetValue(key, out var value))
			{
				return value;
			}
			FLVER2.BufferLayout item = new FLVER2.BufferLayout
			{
				new FLVER.LayoutMember(type, sem, idx, stream, 0)
			};
			flver.BufferLayouts.Add(item);
			value = flver.BufferLayouts.Count - 1;
			layoutCache[key] = value;
			return value;
		}
	}

	public static byte[] VbsBndBytesForTest(byte[] bndBytes, Action<string> log)
	{
		ApplyVbsToBndBytes(bndBytes, out var outBytes, log);
		return outBytes;
	}

	private static int ApplyVbsToBndBytes(byte[] bndBytes, out byte[] outBytes, Action<string> log)
	{
		BND4 bND = MountedSoulsFile<BND4>.Read(bndBytes);
		int num = 0;
		foreach (BinderFile file in bND.Files)
		{
			if (file.Name != null && file.Name.EndsWith(".flver", StringComparison.OrdinalIgnoreCase))
			{
				try
				{
					FLVER2 fLVER = SoulsFile<FLVER2>.Read(file.Bytes.ToArray());
					ApplyVbsToFlver(fLVER);
					file.Bytes = fLVER.Write();
					num++;
				}
				catch (Exception ex)
				{
					log?.Invoke(" (FLVER processing failed: " + Path.GetFileName(file.Name) + " - " + ex.Message + ")");
				}
			}
		}
		outBytes = bND.Write();
		return num;
	}

	public static void BatchModifyVbs(string dir, string outDir, Action<string> log)
	{
		log("[1/3] Recursively searching for geombnd files...");
		List<string> list = new List<string>();
		int dirCount = 0;
		WalkDir(dir, list, ref dirCount, log);
		List<string> list2 = list.Where((string f) =>
		{
			string text = Path.GetFileName(f).ToLowerInvariant();
			return text.Contains(".geombnd") && text.StartsWith("aeg");
		}).ToList();
		log($" Found {list2.Count} AEG geombnd files (scanned {list.Count} files in total)");
		if (list2.Count == 0)
		{
			log(" No geombnd files found. Please confirm the directory is correct (it should contain AEGxxx_xxx.geombnd.dcx).");
			return;
		}
		log("[2/3] Modifying VBS one by one...");
		int num = 0;
		int num2 = 0;
		int num3 = 0;
		foreach (string item in list2)
		{
			string fileName = Path.GetFileName(item);
			try
			{
				BND4 bND = MountedSoulsFile<BND4>.Read(item);
				DCX.Type compression = bND.Compression;
				int num4 = 0;
				foreach (BinderFile file in bND.Files)
				{
					if (file.Name != null && file.Name.EndsWith(".flver", StringComparison.OrdinalIgnoreCase))
					{
						FLVER2 fLVER = SoulsFile<FLVER2>.Read(file.Bytes.ToArray());
						ApplyVbsToFlver(fLVER);
						file.Bytes = fLVER.Write();
						num4++;
					}
				}
				bND.Compression = compression;
				string path = MakeRelativeGeneric(item, dir);
				string path2 = Path.Combine(outDir, path);
				Directory.CreateDirectory(Path.GetDirectoryName(path2));
				bND.Write(path2);
				num++;
				num3 += num4;
				if (num <= 10 || num % 50 == 0)
				{
					log($"      [{num}] {fileName} (changed {num4} FLVERs)");
				}
			}
			catch (Exception ex)
			{
				num2++;
				log(" Failed: " + fileName + " - " + ex.Message);
				if (ex.Message.Contains("oo2core") || ex.Message.Contains("Oodle"))
				{
					log(" => oo2core_9_win64.dll must be placed next to this program to read and write KRAK-compressed geombnd.");
					return;
				}
			}
		}
		log("[3/3] Done.");
		log($" Succeeded: {num} files (changed {num3} FLVERs in total)" + ((num2 > 0) ? $", failed: {num2} files" : ""));
		log(" Target VBS: Position(Float3) + Normal(UByte4) + Tangent(UByte4) + BoneIndices(UShort2) + UV(Short4), buffer indices 0/1/2/4/7");
	}

	public static byte[] ConvertMapPieceFlverForTest(byte[] flverBytes)
	{
		FLVER2 fLVER = SoulsFile<FLVER2>.Read(flverBytes);
		ApplyVbsToFlver(fLVER, dropColor: true);
		return fLVER.Write();
	}

	public static void ConvertMapPieces(string inputDir, string outputDir, string targetMapId, Action<string> log)
	{
		log("[1/3] Searching for mapbnd files...");
		List<string> list = new List<string>();
		int dirCount = 0;
		WalkDir(inputDir, list, ref dirCount, log);
		List<string> list2 = list.Where((string f) => Path.GetFileName(f).ToLowerInvariant().Contains(".mapbnd")).ToList();
		log($" Found {list2.Count} mapbnd files (scanned {list.Count} files in total)");
		if (list2.Count == 0)
		{
			log(" No mapbnd files found (should contain mXX_XX_XX_XX_NNNNNN.mapbnd.dcx).");
			return;
		}
		targetMapId = targetMapId.Trim();
		if (targetMapId.StartsWith("m"))
		{
			targetMapId = targetMapId.Substring(1);
		}
		log("[2/3] Converting one by one (FLVER Elden Ring→Nightreign vertex format, dropping vertex colors + changing map prefix to m" + targetMapId + ")...");
		int num = 0;
		int num2 = 0;
		int num3 = 0;
		foreach (string item in list2)
		{
			string fileName = Path.GetFileName(item);
			Match match = Regex.Match(fileName, "m(\\d\\d_\\d\\d_\\d\\d_\\d\\d)");
			if (!match.Success)
			{
				log(" Skipped (cannot recognize map ID): " + fileName);
				continue;
			}
			string value = match.Groups[1].Value;
			try
			{
				BND4 bND = MountedSoulsFile<BND4>.Read(item);
				DCX.Type compression = bND.Compression;
				int num4 = 0;
				foreach (BinderFile file in bND.Files)
				{
					if (file.Name != null && file.Name.EndsWith(".flver", StringComparison.OrdinalIgnoreCase))
					{
						FLVER2 fLVER = SoulsFile<FLVER2>.Read(file.Bytes.ToArray());
						ApplyVbsToFlver(fLVER, dropColor: true);
						file.Bytes = fLVER.Write();
						num4++;
					}
					if (file.Name != null)
					{
						file.Name = file.Name.Replace("m" + value, "m" + targetMapId);
					}
				}
				bND.Compression = compression;
				string text = fileName.Replace("m" + value, "m" + targetMapId);
				string path = Path.Combine(outputDir, text);
				Directory.CreateDirectory(outputDir);
				bND.Write(path);
				num++;
				num3 += num4;
				if (num <= 10 || num % 50 == 0)
				{
					log($"      [{num}] {fileName} -> {text} (converted {num4} FLVERs)");
				}
			}
			catch (Exception ex)
			{
				num2++;
				log(" Failed: " + fileName + " - " + ex.Message);
				if (ex.Message.Contains("oo2core") || ex.Message.Contains("Oodle"))
				{
					log(" => oo2core_9_win64.dll must be placed next to this program to read and write KRAK-compressed mapbnd.");
					return;
				}
			}
		}
		log("[3/3] Done.");
		log($" Succeeded: {num} files (converted {num3} FLVERs in total)" + ((num2 > 0) ? $", failed: {num2} files" : ""));
		log(" Copy all the output m" + targetMapId + "_NNNNNN.mapbnd.dcx files into the Nightreign m60 map folder (the buildings will then display normally without crashing).");
	}

	private static string MakeRelativeGeneric(string fullPath, string root)
	{
		try
		{
			string text = Path.GetFullPath(root).TrimEnd(new char[2] { '\\', '/' });
			string fullPath2 = Path.GetFullPath(fullPath);
			if (fullPath2.StartsWith(text, StringComparison.OrdinalIgnoreCase))
			{
				string text2 = fullPath2.Substring(text.Length).TrimStart(new char[2] { '\\', '/' });
				if (!string.IsNullOrEmpty(text2))
				{
					return text2;
				}
			}
		}
		catch
		{
		}
		return Path.GetFileName(fullPath);
	}

	private static (HashSet<string> matbins, HashSet<string> aetRefs) ExtractMatInfoFromGeombnd(string geombndPath, ref bool oodleFail, ref int probed)
	{
		HashSet<string> hashSet = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
		HashSet<string> hashSet2 = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
		try
		{
			foreach (BinderFile file in MountedSoulsFile<BND4>.Read(geombndPath).Files)
			{
				if (file.Name == null || !file.Name.EndsWith(".flver", StringComparison.OrdinalIgnoreCase))
				{
					continue;
				}
				foreach (FLVER2.Material material in SoulsFile<FLVER2>.Read(file.Bytes.ToArray()).Materials)
				{
					if (!string.IsNullOrEmpty(material.MTD))
					{
						hashSet.Add(Path.GetFileNameWithoutExtension(material.MTD));
					}
					foreach (FLVER2.Texture texture in material.Textures)
					{
						if (string.IsNullOrEmpty(texture.Path))
						{
							continue;
						}
						foreach (Match item in AetRefRe.Matches(texture.Path))
						{
							hashSet2.Add(("aet" + item.Groups[1].Value + "_" + item.Groups[2].Value).ToLowerInvariant());
						}
					}
				}
			}
		}
		catch (Exception ex)
		{
			if (probed == 0 && (ex.Message.Contains("oo2core") || ex.Message.Contains("Oodle")))
			{
				oodleFail = true;
			}
		}
		probed++;
		return (matbins: hashSet, aetRefs: hashSet2);
	}

	private static HashSet<string> CollectAetFromMatbins(string matbinbndPath, HashSet<string> neededNames, Action<string> log)
	{
		HashSet<string> hashSet = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
		try
		{
			BND4 bND = MountedSoulsFile<BND4>.Read(matbinbndPath);
			Dictionary<string, BinderFile> dictionary = new Dictionary<string, BinderFile>(StringComparer.OrdinalIgnoreCase);
			foreach (BinderFile file in bND.Files)
			{
				if (file.Name != null)
				{
					string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(file.Name);
					if (!dictionary.ContainsKey(fileNameWithoutExtension))
					{
						dictionary[fileNameWithoutExtension] = file;
					}
				}
			}
			int num = 0;
			foreach (string neededName in neededNames)
			{
				if (!dictionary.TryGetValue(neededName, out var value))
				{
					continue;
				}
				try
				{
					foreach (MATBIN.Sampler sampler in SoulsFile<MATBIN>.Read(value.Bytes.ToArray()).Samplers)
					{
						if (string.IsNullOrEmpty(sampler.Path))
						{
							continue;
						}
						foreach (Match item in AetRefRe.Matches(sampler.Path))
						{
							hashSet.Add(("aet" + item.Groups[1].Value + "_" + item.Groups[2].Value).ToLowerInvariant());
						}
					}
					num++;
				}
				catch
				{
				}
			}
			log($" matbin matched {num}/{neededNames.Count} materials, got {hashSet.Count} exact texture references");
		}
		catch (Exception ex)
		{
			log(" (matbin database read failed, skipping shared texture completion: " + ex.Message + ")");
		}
		return hashSet;
	}

	public static void MergeEmevd(string basePath, string addPath, string outPath, Action<string> log, string remapPath = null)
	{
		log("[1/4] Reading base events (Nightreign): " + Path.GetFileName(basePath));
		EMEVD eMEVD = SoulsFile<EMEVD>.Read(basePath);
		log(" Reading events to add (Elden Ring): " + Path.GetFileName(addPath));
		EMEVD eMEVD2 = SoulsFile<EMEVD>.Read(addPath);
		log($" Base: {eMEVD.Events.Count} events ({eMEVD.Format}); Elden Ring: {eMEVD2.Events.Count} events ({eMEVD2.Format})");
		if (!string.IsNullOrEmpty(remapPath) && File.Exists(remapPath))
		{
			Dictionary<int, int> dictionary = LoadRemap(remapPath);
			int value = ApplyCommonFuncRemap(eMEVD2, dictionary);
			log($" Applying common_func remap table ({dictionary.Count} entries): changed {value} InitializeCommonEvent calls in the Elden Ring events");
		}
		ConvertErBonfiresToNr(eMEVD2, log);
		int value2 = MergeEmevdObjects(eMEVD, eMEVD2, log);
		log("[4/4] Writing the merged events...");
		eMEVD.Write(outPath);
		log($" ✓ Done: total {value2} events -> {Path.GetFileName(outPath)}");
		if (string.IsNullOrEmpty(remapPath))
		{
			log(" Note: the InitializeCommonEvent calls in Elden Ring events call the common events of common_func.emevd;");
			log(" it is recommended to first use the 'Merge common_func' function to generate the merged common_func and the remap table, then select the remap table here.");
		}
	}

	public static void MergeCommonFunc(string baseNrPath, string addErPath, string outPath, string remapPath, Action<string> log)
	{
		log("[1/4] Reading Nightreign common_func: " + Path.GetFileName(baseNrPath));
		EMEVD eMEVD = SoulsFile<EMEVD>.Read(baseNrPath);
		log(" Reading Elden Ring common_func: " + Path.GetFileName(addErPath));
		EMEVD eMEVD2 = SoulsFile<EMEVD>.Read(addErPath);
		log($" Nightreign: {eMEVD.Events.Count} functions; Elden Ring: {eMEVD2.Events.Count} functions");
		if (eMEVD.Format != eMEVD2.Format)
		{
			log($" ⚠ Formats differ ({eMEVD.Format} vs {eMEVD2.Format}), still trying to merge.");
		}
		HashSet<long> hashSet = new HashSet<long>(eMEVD.Events.Select((EMEVD.Event e) => e.ID));
		HashSet<long> hashSet2 = new HashSet<long>(hashSet);
		foreach (EMEVD.Event @event in eMEVD2.Events)
		{
			hashSet2.Add(@event.ID);
		}
		long num = hashSet2.Max() + 1;
		log("[2/4] Detecting conflicts and renumbering conflicting Elden Ring functions...");
		Dictionary<long, long> dictionary = new Dictionary<long, long>();
		int num2 = 0;
		int num3 = 0;
		foreach (EMEVD.Event event2 in eMEVD2.Events)
		{
			if (hashSet.Contains(event2.ID))
			{
				long num4 = num++;
				if (num4 > int.MaxValue)
				{
					log(" ✗ Renumbering exceeds the int32 range, cannot continue (too many conflicts).");
					return;
				}
				dictionary[event2.ID] = num4;
				event2.ID = num4;
				num2++;
			}
			else
			{
				num3++;
			}
		}
		log($" Elden Ring: {num3} functions not in Nightreign (added directly), {num2} conflicts (renumbered to {((dictionary.Count > 0) ? (dictionary.Values.Min() + "~" + dictionary.Values.Max()) : "-")})");
		int num5 = ApplyCommonFuncRemap(eMEVD2, dictionary);
		if (num5 > 0)
		{
			log($" (also fixed {num5} internal mutual calls within the Elden Ring common_func)");
		}
		log("[3/4] Adding Elden Ring functions to Nightreign common_func...");
		foreach (EMEVD.Event event3 in eMEVD2.Events)
		{
			eMEVD.Events.Add(event3);
		}
		log("[4/4] Writing the merged common_func and the remap table...");
		eMEVD.Write(outPath);
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.AppendLine("# common_func remap table: Elden Ring old ID -> new ID after renumbering");
		stringBuilder.AppendLine("# Select this table when merging map events, and Elden Ring map calls to these functions will automatically be changed to the new IDs");
		foreach (KeyValuePair<long, long> item in dictionary.OrderBy((KeyValuePair<long, long> k) => k.Key))
		{
			StringBuilder stringBuilder2 = stringBuilder;
			StringBuilder.AppendInterpolatedStringHandler handler = new StringBuilder.AppendInterpolatedStringHandler(1, 2, stringBuilder2);
			handler.AppendFormatted(item.Key);
			handler.AppendLiteral(" ");
			handler.AppendFormatted(item.Value);
			stringBuilder2.AppendLine(ref handler);
		}
		File.WriteAllText(remapPath, stringBuilder.ToString());
		log($" ✓ Merged common_func has {eMEVD.Events.Count} functions in total -> {Path.GetFileName(outPath)}");
		log($" ✓ Remap table ({dictionary.Count} entries) -> {Path.GetFileName(remapPath)}");
		log(" Next step: when merging maps with the 'Merge events' function, select this remap table.");
		log(" Note: renumbering gives Elden Ring functions independent IDs in Nightreign so they are called correctly; however, if a function internally references");
		log(" Elden Ring-specific SpEffect/params/animations etc., they may still need separate adaptation in Nightreign (game data layer).");
	}

	public static void MergeMapCollision(string baseHBhd, string addHBhd, string baseLBhd, string addLBhd, string outHBhd, string outLBhd, string remapPath, Action<string> log, bool dropHighPieces = false)
	{
		log("[1/4] Reading collision archives...");
		BXF4 bXF = ReadBxf(baseHBhd);
		BXF4 bXF2 = ReadBxf(addHBhd);
		BXF4 bXF3 = null;
		BXF4 bXF4 = null;
		bool flag = !string.IsNullOrEmpty(baseLBhd) && !string.IsNullOrEmpty(addLBhd);
		if (flag)
		{
			bXF3 = ReadBxf(baseLBhd);
			bXF4 = ReadBxf(addLBhd);
		}
		log($" Nightreign (base) high collision {bXF.Files.Count} entries; Elden Ring (added) high collision {bXF2.Files.Count} entries" + (flag ? $"; low collision Nightreign{bXF3.Files.Count}/Elden Ring{bXF4.Files.Count}" : " (no low collision given)"));
		string text = ExtractMapId(bXF.Files.Select((BinderFile f) => f.Name)) ?? (flag ? ExtractMapId(bXF3.Files.Select((BinderFile f) => f.Name)) : null);
		string text2 = ExtractMapId(bXF2.Files.Select((BinderFile f) => f.Name)) ?? (flag ? ExtractMapId(bXF4.Files.Select((BinderFile f) => f.Name)) : null);
		if (text == null || text2 == null)
		{
			log(" ✗ Cannot recognize the map ID of the collision archive (abnormal entry name format), aborting.");
			return;
		}
		log(" Nightreign map ID=" + text + ", Elden Ring map ID=" + text2);
		log($" → Elden Ring entry prefixes will be changed from m{text2} to m{text} (the game looks for collision by 'current map prefix + number')");
		HashSet<string> hashSet = new HashSet<string>(from f in bXF.Files
			select CollisionNum(f.Name) into n
			where n != null
			select n);
		HashSet<string> hashSet2 = new HashSet<string>(hashSet);
		foreach (BinderFile file in bXF2.Files)
		{
			string text3 = CollisionNum(file.Name);
			if (text3 != null)
			{
				hashSet2.Add(text3);
			}
		}
		int num = 900000;
		while (hashSet2.Contains(num.ToString("D6")))
		{
			num++;
		}
		log("[2/4] Detecting number collisions...");
		Dictionary<string, string> dictionary = new Dictionary<string, string>();
		foreach (BinderFile file2 in bXF2.Files)
		{
			string text4 = CollisionNum(file2.Name);
			if (text4 != null && hashSet.Contains(text4) && !dictionary.ContainsKey(text4))
			{
				string text5 = num++.ToString("D6");
				while (hashSet2.Contains(text5))
				{
					text5 = num++.ToString("D6");
				}
				hashSet2.Add(text5);
				dictionary[text4] = text5;
			}
		}
		if (dictionary.Count > 0)
		{
			log($"{dictionary.Count} numbers collide with Nightreign and were renumbered (the MSB needs to be changed when merging maps)");
		}
		else
		{
			log(" Elden Ring numbers do not collide with Nightreign, the MSB does not need changes");
		}
		log("[3/4] Merging (Elden Ring entries get a new prefix + are added into Nightreign; Nightreign's compendium is kept, the Havok type table hashes on both sides are identical and compatible)...");
		int value = MergeBxfReprefix(bXF, bXF2, text, text2, dictionary, dropHighPieces, out var dropped);
		int value2 = 0;
		int dropped2 = 0;
		if (flag)
		{
			value2 = MergeBxfReprefix(bXF3, bXF4, text, text2, dictionary, dropHighPieces, out dropped2);
		}
		if (dropHighPieces)
		{
			log($" Removed 9xxxxx high tiles: high collision {dropped}" + (flag ? $", low collision{dropped2}" : "") + " (they reference a compendium outside the archive; keeping them would cause Smithbox/the game to report errors; cost: a few small areas at high places have no collision)");
		}
		log("[4/4] Writing the merged collision archive + remap table...");
		WriteBxf(bXF, outHBhd);
		if (flag)
		{
			WriteBxf(bXF3, outLBhd);
		}
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.AppendLine("# Collision model remap table (has content only when numbers collide): old name new name");
		stringBuilder.AppendLine("# If it has content, select this table when merging maps (MSB)");
		foreach (KeyValuePair<string, string> item in dictionary.OrderBy((KeyValuePair<string, string> k) => k.Key))
		{
			StringBuilder stringBuilder2 = stringBuilder;
			StringBuilder stringBuilder3 = stringBuilder2;
			StringBuilder.AppendInterpolatedStringHandler handler = new StringBuilder.AppendInterpolatedStringHandler(3, 2, stringBuilder2);
			handler.AppendLiteral("h");
			handler.AppendFormatted(item.Key);
			handler.AppendLiteral(" h");
			handler.AppendFormatted(item.Value);
			stringBuilder3.AppendLine(ref handler);
			stringBuilder2 = stringBuilder;
			StringBuilder stringBuilder4 = stringBuilder2;
			handler = new StringBuilder.AppendInterpolatedStringHandler(3, 2, stringBuilder2);
			handler.AppendLiteral("l");
			handler.AppendFormatted(item.Key);
			handler.AppendLiteral(" l");
			handler.AppendFormatted(item.Value);
			stringBuilder4.AppendLine(ref handler);
		}
		File.WriteAllText(remapPath, stringBuilder.ToString());
		log($" ✓ After merge: high collision {bXF.Files.Count} entries (Elden Ring+{value})" + (flag ? $", low collision {bXF3.Files.Count} entries (Elden Ring+{value2})" : "") + " -> " + Path.GetFileName(outHBhd));
		log($" ✓ Remap table ({dictionary.Count} pairs) -> {Path.GetFileName(remapPath)}");
		if (dictionary.Count > 0)
		{
			log(" ⚠ Numbers collide: when using 'Merge map', select this collision remap table so that the MSB collision references line up.");
		}
		else
		{
			log(" ✓ No collision of numbers: the MSB does not need changes, and the collision remap table can be left empty when merging maps. Use the merged collision archive directly.");
		}
	}

	private static string ExtractMapId(IEnumerable<string> names)
	{
		if (names == null)
		{
			return null;
		}
		foreach (string name in names)
		{
			if (name != null)
			{
				Match match = Regex.Match(name, "m(\\d\\d_\\d\\d_\\d\\d_\\d\\d)");
				if (match.Success)
				{
					return match.Groups[1].Value;
				}
			}
		}
		return null;
	}

	private static BXF4 ReadBxf(string bhdPath)
	{
		string bdtPath = BdtFromBhd(bhdPath);
		return BXF4.Read(bhdPath, bdtPath);
	}

	private static void WriteBxf(BXF4 b, string bhdPath)
	{
		string bdtPath = BdtFromBhd(bhdPath);
		string directoryName = Path.GetDirectoryName(bhdPath);
		if (!string.IsNullOrEmpty(directoryName))
		{
			Directory.CreateDirectory(directoryName);
		}
		b.Write(bhdPath, bdtPath);
	}

	private static string BdtFromBhd(string p)
	{
		if (p.EndsWith(".hkxbhd", StringComparison.OrdinalIgnoreCase))
		{
			return p.Substring(0, p.Length - 7) + ".hkxbdt";
		}
		return Path.ChangeExtension(p, ".hkxbdt");
	}

	private static string CollisionNum(string name)
	{
		if (name == null || name.Contains("compendium"))
		{
			return null;
		}
		Match match = Regex.Match(name, "_(\\d{6})\\.hkx");
		if (!match.Success)
		{
			return null;
		}
		return match.Groups[1].Value;
	}

	private static int MergeBxfReprefix(BXF4 baseB, BXF4 addB, string baseMapId, string addMapId, Dictionary<string, string> numRemap, bool dropHighPieces, out int dropped)
	{
		dropped = 0;
		int num = ((baseB.Files.Count > 0) ? (baseB.Files.Max((BinderFile f) => f.ID) + 1) : 0);
		int num2 = 0;
		foreach (BinderFile file in addB.Files)
		{
			if (file.Name != null && file.Name.Contains("compendium"))
			{
				continue;
			}
			string text = CollisionNum(file.Name);
			if (text != null && int.TryParse(text, out var result) && result >= 900000)
			{
				dropped++;
				continue;
			}
			string text2 = file.Name?.Replace(addMapId, baseMapId);
			if (text != null && numRemap.TryGetValue(text, out var value))
			{
				text2 = Regex.Replace(text2, "_" + text + "\\.hkx", "_" + value + ".hkx");
			}
			baseB.Files.Add(new BinderFile(file.Flags, num++, text2, file.Bytes)
			{
				CompressionType = file.CompressionType
			});
			num2++;
		}
		return num2;
	}

	private static Dictionary<string, string> LoadNameRemap(string path)
	{
		Dictionary<string, string> dictionary = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
		string[] array = File.ReadAllLines(path);
		for (int i = 0; i < array.Length; i++)
		{
			string text = array[i].Trim();
			if (text.Length != 0 && !text.StartsWith("#"))
			{
				string[] array2 = text.Split(new char[4] { ' ', '\t', ',', '=' }, StringSplitOptions.RemoveEmptyEntries);
				if (array2.Length >= 2)
				{
					dictionary[array2[0]] = array2[1];
				}
			}
		}
		return dictionary;
	}

	private static int ApplyCollisionModelRemap(MSB_NR map, Dictionary<string, string> remap)
	{
		if (remap == null || remap.Count == 0)
		{
			return 0;
		}
		int num = 0;
		foreach (MSB_NR.Model.Collision collision in map.Models.Collisions)
		{
			if (collision.Name != null && remap.TryGetValue(collision.Name, out var value))
			{
				collision.Name = value;
				num++;
			}
		}
		foreach (MSB_NR.Part.Collision collision2 in map.Parts.Collisions)
		{
			if (collision2.ModelName != null && remap.TryGetValue(collision2.ModelName, out var value2))
			{
				collision2.ModelName = value2;
				num++;
			}
		}
		return num;
	}

	private static Dictionary<int, int> LoadRemap(string path)
	{
		Dictionary<int, int> dictionary = new Dictionary<int, int>();
		string[] array = File.ReadAllLines(path);
		for (int i = 0; i < array.Length; i++)
		{
			string text = array[i].Trim();
			if (text.Length != 0 && !text.StartsWith("#"))
			{
				string[] array2 = text.Split(new char[4] { ' ', '\t', ',', '=' }, StringSplitOptions.RemoveEmptyEntries);
				if (array2.Length >= 2 && int.TryParse(array2[0], out var result) && int.TryParse(array2[1], out var result2))
				{
					dictionary[result] = result2;
				}
			}
		}
		return dictionary;
	}

	private static int ApplyCommonFuncRemap(EMEVD e, Dictionary<int, int> remap)
	{
		if (remap == null || remap.Count == 0)
		{
			return 0;
		}
		int num = 0;
		foreach (EMEVD.Event @event in e.Events)
		{
			foreach (EMEVD.Instruction instruction in @event.Instructions)
			{
				if (instruction.Bank == 2000 && instruction.ID == 6 && instruction.ArgData != null && instruction.ArgData.Length >= 8)
				{
					int key = BitConverter.ToInt32(instruction.ArgData, 4);
					if (remap.TryGetValue(key, out var value))
					{
						Array.Copy(BitConverter.GetBytes(value), 0, instruction.ArgData, 4, 4);
						num++;
					}
				}
			}
		}
		return num;
	}

	private static int ApplyCommonFuncRemap(EMEVD e, Dictionary<long, long> remapL)
	{
		Dictionary<int, int> remap = remapL.ToDictionary((KeyValuePair<long, long> k) => (int)k.Key, (KeyValuePair<long, long> v) => (int)v.Value);
		return ApplyCommonFuncRemap(e, remap);
	}

	private static int ConvertErBonfiresToNr(EMEVD e, Action<string> log)
	{
		int num = 0;
		List<long> list = new List<long>();
		foreach (EMEVD.Event @event in e.Events)
		{
			HashSet<long> hashSet = ((@event.Parameters != null) ? new HashSet<long>(@event.Parameters.Select((EMEVD.Parameter p) => p.InstructionIndex)) : new HashSet<long>());
			for (int num2 = 0; num2 < @event.Instructions.Count; num2++)
			{
				EMEVD.Instruction instruction = @event.Instructions[num2];
				if (instruction.Bank == 2009 && instruction.ID == 3 && instruction.ArgData != null && instruction.ArgData.Length >= 24 && !hashSet.Contains(num2))
				{
					int num3 = BitConverter.ToInt32(instruction.ArgData, 4);
					byte[] array = new byte[12];
					BitConverter.GetBytes(num3).CopyTo(array, 0);
					BitConverter.GetBytes(0).CopyTo(array, 4);
					Array.Copy(instruction.ArgData, 20, array, 8, 4);
					instruction.Bank = 2009;
					instruction.ID = 12;
					instruction.ArgData = array;
					num++;
					list.Add(num3);
				}
			}
		}
		if (num > 0 && log != null)
		{
			List<long> list2 = list.Distinct().ToList();
			log($" Grace: converting {num} Elden Ring RegisterBonfire(2009[3]) to Nightreign format(2009[12]); total asset EIDs: {list2.Count}: {string.Join(",", list2.Take(20))}{((list2.Count > 20) ? " ..." : "")}");
		}
		return num;
	}

	private static int MergeEmevdObjects(EMEVD baseE, EMEVD addE, Action<string> log)
	{
		if (baseE.Format != addE.Format)
		{
			log($" ⚠ Event formats differ ({baseE.Format} vs {addE.Format}), still trying to merge, but they may be incompatible.");
		}
		HashSet<long> baseIds = new HashSet<long>(baseE.Events.Select((EMEVD.Event e) => e.ID));
		EMEVD.Event obj = baseE.Events.FirstOrDefault((EMEVD.Event e) => e.ID == 0);
		EMEVD.Event obj2 = addE.Events.FirstOrDefault((EMEVD.Event e) => e.ID == 0);
		HashSet<long> hashSet = new HashSet<long>(from e in addE.Events
			where e.ID != 0L && baseIds.Contains(e.ID)
			select e.ID);
		if (hashSet.Count > 0)
		{
			log($" ⚠ {hashSet.Count} Elden Ring event IDs conflict with the base and will be skipped (the base ones are kept): {string.Join(",", hashSet.Take(15))}{((hashSet.Count > 15) ? " ..." : "")}");
		}
		log("[2/4] Merging event 0 (constructor: so that Elden Ring events get initialized)...");
		int num = 0;
		if (obj2 != null)
		{
			if (obj == null)
			{
				obj = new EMEVD.Event(0L, obj2.RestBehavior);
				baseE.Events.Insert(0, obj);
				log(" (base has no event 0, created a new one)");
			}
			int count = obj.Instructions.Count;
			List<EMEVD.Instruction> list = new List<EMEVD.Instruction>();
			Dictionary<int, int> dictionary = new Dictionary<int, int>();
			for (int num2 = 0; num2 < obj2.Instructions.Count; num2++)
			{
				EMEVD.Instruction instruction = obj2.Instructions[num2];
				if (hashSet.Count > 0 && instruction.Bank == 2000 && instruction.ID == 0 && instruction.ArgData != null && instruction.ArgData.Length >= 8)
				{
					long item = BitConverter.ToInt32(instruction.ArgData, 4);
					if (hashSet.Contains(item))
					{
						continue;
					}
				}
				dictionary[num2] = count + list.Count;
				list.Add(instruction);
				num++;
			}
			obj.Instructions.AddRange(list);
			if (obj2.Parameters != null)
			{
				foreach (EMEVD.Parameter parameter in obj2.Parameters)
				{
					if (dictionary.TryGetValue((int)parameter.InstructionIndex, out var value))
					{
						parameter.InstructionIndex = value;
						obj.Parameters.Add(parameter);
					}
				}
			}
		}
		else
		{
			log(" (Elden Ring has no event 0, skipping constructor merge)");
		}
		log($" Event 0: Elden Ring appended {num} instructions");
		log("[3/4] Adding the other Elden Ring events...");
		int num3 = 0;
		int num4 = 0;
		foreach (EMEVD.Event @event in addE.Events)
		{
			if (@event.ID != 0L)
			{
				if (hashSet.Contains(@event.ID))
				{
					num4++;
					continue;
				}
				baseE.Events.Add(@event);
				num3++;
			}
		}
		log($" Added {num3} Elden Ring events" + ((num4 > 0) ? $", skipped conflicts: {num4}" : ""));
		return baseE.Events.Count;
	}

	private static bool ReadPartPositions(string msbPath, out Dictionary<string, (float x, float y, float z)> colPos, out Dictionary<string, (float x, float y, float z)> bldPos)
	{
		colPos = new Dictionary<string, (float, float, float)>();
		bldPos = new Dictionary<string, (float, float, float)>();
		try
		{
			MSB_NR mSB_NR = SoulsFile<MSB_NR>.Read(msbPath);
			foreach (MSB_NR.Part.Collision collision in mSB_NR.Parts.Collisions)
			{
				if (collision.Name != null)
				{
					colPos[collision.Name] = (collision.Position.X, collision.Position.Y, collision.Position.Z);
				}
			}
			foreach (MSB_NR.Part.Asset asset in mSB_NR.Parts.Assets)
			{
				if (asset.Name != null)
				{
					bldPos[asset.Name] = (asset.Position.X, asset.Position.Y, asset.Position.Z);
				}
			}
			foreach (MSB_NR.Part.MapPiece mapPiece in mSB_NR.Parts.MapPieces)
			{
				if (mapPiece.Name != null)
				{
					bldPos[mapPiece.Name] = (mapPiece.Position.X, mapPiece.Position.Y, mapPiece.Position.Z);
				}
			}
			return true;
		}
		catch
		{
			try
			{
				MSBE mSBE = SoulsFile<MSBE>.Read(msbPath);
				foreach (MSBE.Part.Collision collision2 in mSBE.Parts.Collisions)
				{
					if (collision2.Name != null)
					{
						colPos[collision2.Name] = (collision2.Position.X, collision2.Position.Y, collision2.Position.Z);
					}
				}
				foreach (MSBE.Part.Asset asset2 in mSBE.Parts.Assets)
				{
					if (asset2.Name != null)
					{
						bldPos[asset2.Name] = (asset2.Position.X, asset2.Position.Y, asset2.Position.Z);
					}
				}
				foreach (MSBE.Part.MapPiece mapPiece2 in mSBE.Parts.MapPieces)
				{
					if (mapPiece2.Name != null)
					{
						bldPos[mapPiece2.Name] = (mapPiece2.Position.X, mapPiece2.Position.Y, mapPiece2.Position.Z);
					}
				}
				return true;
			}
			catch
			{
				return false;
			}
		}
	}

	private static ((float dx, float dy, float dz) best, int bestCount, List<((float dx, float dy, float dz) d, int n)> clusters) ClusterDeltas(List<(float dx, float dy, float dz)> deltas)
	{
		Dictionary<string, ((float, float, float), int)> dictionary = new Dictionary<string, ((float, float, float), int)>();
		foreach (var delta in deltas)
		{
			string key = $"{MathF.Round(delta.dx / 0.5f) * 0.5f:F1},{MathF.Round(delta.dy / 0.5f) * 0.5f:F1},{MathF.Round(delta.dz / 0.5f) * 0.5f:F1}";
			if (dictionary.TryGetValue(key, out var value))
			{
				dictionary[key] = (value.Item1, value.Item2 + 1);
			}
			else
			{
				dictionary[key] = (delta, 1);
			}
		}
		List<((float, float, float), int)> list = dictionary.Values.OrderByDescending((((float dx, float dy, float dz) d, int n) x) => x.n).ToList();
		((float, float, float), int) tuple = list[0];
		return (best: tuple.Item1, bestCount: tuple.Item2, clusters: list);
	}

	public static (float dx, float dy, float dz, int matched) ComputeBuildingOffset(string mergedMsbPath, string origMsbPath, Action<string> log)
	{
		if (!ReadPartPositions(mergedMsbPath, out Dictionary<string, (float, float, float)> colPos, out Dictionary<string, (float, float, float)> bldPos))
		{
			log(" Warning: failed to read the merged MSB, offset set to 0");
			return (dx: 0f, dy: 0f, dz: 0f, matched: 0);
		}
		if (!ReadPartPositions(origMsbPath, out Dictionary<string, (float, float, float)> colPos2, out Dictionary<string, (float, float, float)> bldPos2))
		{
			log(" Warning: failed to read the original MSB, offset set to 0");
			return (dx: 0f, dy: 0f, dz: 0f, matched: 0);
		}
		List<(float, float, float)> list = new List<(float, float, float)>();
		foreach (KeyValuePair<string, (float, float, float)> item in colPos)
		{
			if (colPos2.TryGetValue(item.Key, out var value))
			{
				list.Add((item.Value.Item1 - value.Item1, item.Value.Item2 - value.Item2, item.Value.Item3 - value.Item3));
			}
		}
		List<(float, float, float)> list2;
		string value2;
		if (list.Count > 0)
		{
			list2 = list;
			value2 = "collision pieces";
		}
		else
		{
			List<(float, float, float)> list3 = new List<(float, float, float)>();
			foreach (KeyValuePair<string, (float, float, float)> item2 in bldPos)
			{
				if (bldPos2.TryGetValue(item2.Key, out var value3))
				{
					list3.Add((item2.Value.Item1 - value3.Item1, item2.Value.Item2 - value3.Item2, item2.Value.Item3 - value3.Item3));
				}
			}
			if (list3.Count == 0)
			{
				log(" No same-name parts matched, offset set to 0");
				return (dx: 0f, dy: 0f, dz: 0f, matched: 0);
			}
			list2 = list3;
			value2 = "asset pieces (no collision pieces matched, fallback)";
		}
		var (tuple2, num, list4) = ClusterDeltas(list2);
		log($" Calculating offset using [{value2}]: matched {list2.Count}, main cluster {num} ({num * 100 / list2.Count}%)");
		log($" Offset = ({tuple2.Item1:F3}, {tuple2.Item2:F3}, {tuple2.Item3:F3})");
		if (list4.Count > 1)
		{
			log($" ⚠ Detected {list4.Count} different displacement clusters (meaning different collisions were moved by different distances):");
			foreach (var item3 in list4.Take(5))
			{
				log($"        ({item3.Item1.Item1:F1},{item3.Item1.Item2:F1},{item3.Item1.Item3:F1}) × {item3.Item2}");
			}
			if (num < list2.Count)
			{
				log($" This time only the main cluster is shifted; the {list2.Count - num} collisions outside the main cluster will be slightly off (you can fine-tune those manually).");
			}
		}
		return (dx: tuple2.Item1, dy: tuple2.Item2, dz: tuple2.Item3, matched: list2.Count);
	}

	private static string DeriveBdt2(string bhd)
	{
		if (!bhd.EndsWith(".hkxbhd", StringComparison.OrdinalIgnoreCase))
		{
			return Path.ChangeExtension(bhd, ".hkxbdt");
		}
		return bhd.Substring(0, bhd.Length - 7) + ".hkxbdt";
	}

	private static string DeriveLowFromHigh(string hBhd)
	{
		string? path = Path.GetDirectoryName(hBhd) ?? "";
		string text = Path.GetFileName(hBhd);
		if (text.Length > 0 && text[0] == 'h')
		{
			text = "l" + text.Substring(1);
		}
		return Path.Combine(path, text);
	}

	private static Dictionary<string, (Vector3 pos, Vector3 rot)> ReadCollisionTransforms(string path)
	{
		Dictionary<string, (Vector3, Vector3)> dictionary = new Dictionary<string, (Vector3, Vector3)>();
		try
		{
			foreach (MSB_NR.Part.Collision collision in SoulsFile<MSB_NR>.Read(path).Parts.Collisions)
			{
				if (collision.Name != null)
				{
					dictionary[collision.Name] = (collision.Position, collision.Rotation);
				}
			}
			return dictionary;
		}
		catch
		{
		}
		try
		{
			foreach (MSBE.Part.Collision collision2 in SoulsFile<MSBE>.Read(path).Parts.Collisions)
			{
				if (collision2.Name != null)
				{
					dictionary[collision2.Name] = (collision2.Position, collision2.Rotation);
				}
			}
		}
		catch
		{
		}
		return dictionary;
	}

	private static Vector3 ComputeBuildingOnlyOffset(string mergedPath, string origPath, Action<string> log)
	{
		if (!ReadPartPositions(mergedPath, out Dictionary<string, (float, float, float)> colPos, out Dictionary<string, (float, float, float)> bldPos) || !ReadPartPositions(origPath, out colPos, out Dictionary<string, (float, float, float)> bldPos2))
		{
			log?.Invoke(" ⚠ Failed to read MSB to calculate building offset, D=0");
			return Vector3.Zero;
		}
		List<(float, float, float)> list = new List<(float, float, float)>();
		foreach (KeyValuePair<string, (float, float, float)> item in bldPos)
		{
			if (bldPos2.TryGetValue(item.Key, out var value))
			{
				list.Add((item.Value.Item1 - value.Item1, item.Value.Item2 - value.Item2, item.Value.Item3 - value.Item3));
			}
		}
		if (list.Count == 0)
		{
			log?.Invoke(" ⚠ Buildings not matched, D=0");
			return Vector3.Zero;
		}
		var (tuple2, num, _) = ClusterDeltas(list);
		log?.Invoke($" Calculating offset using buildings (map tiles + assets): matched {list.Count}, main cluster {num} ({num * 100 / list.Count}%)");
		return new Vector3(tuple2.Item1, tuple2.Item2, tuple2.Item3);
	}

	private static (int fixedCount, int rotCount, int unmatched) ApplyCollisionInstanceFix(MSB_NR m, Dictionary<string, (Vector3 pos, Vector3 rot)> m13col, Vector3 D, Action<string> log)
	{
		int num = 0;
		int num2 = 0;
		int num3 = 0;
		foreach (MSB_NR.Part.Collision collision in m.Parts.Collisions)
		{
			if (collision.Name != null && m13col.TryGetValue(collision.Name, out (Vector3, Vector3) value))
			{
				Matrix4x4 matrix = Matrix4x4.CreateRotationX(value.Item2.X * (float)Math.PI / 180f) * Matrix4x4.CreateRotationZ(value.Item2.Z * (float)Math.PI / 180f) * Matrix4x4.CreateRotationY(value.Item2.Y * (float)Math.PI / 180f);
				collision.Rotation = value.Item2;
				collision.Position = value.Item1 + (D - Vector3.Transform(D, matrix));
				num++;
				if (value.Item2.Length() >= 0.01f)
				{
					num2++;
				}
			}
			else
			{
				num3++;
			}
		}
		if (log != null)
		{
			log($" Collision pieces repositioned by P_m13 + (I-R)·D: {num} (of which rotated reused pieces {num2})");
			if (num3 > 0)
			{
				log($" Additionally, {num3} collision pieces had no same-name match in the Elden Ring MSB (mostly Nightreign's built-in collision) and were left unchanged.");
			}
		}
		return (fixedCount: num, rotCount: num2, unmatched: num3);
	}

	public static void FixRotatedCollisionMsb(string mergedMsbPath, string m13MsbPath, string outPath, Action<string> log)
	{
		log("[1/4] Reading original position+rotation of Elden Ring m13 collision pieces: " + Path.GetFileName(m13MsbPath));
		Dictionary<string, (Vector3, Vector3)> dictionary = ReadCollisionTransforms(m13MsbPath);
		int value = dictionary.Values.Count(((Vector3 pos, Vector3 rot) v) => v.rot.Length() >= 0.01f);
		int value2 = dictionary.Values.Count(((Vector3 pos, Vector3 rot) v) => v.rot.Length() < 0.01f && v.pos.Length() >= 0.01f);
		log($" Elden Ring collision pieces {dictionary.Count} (reused with rotation: {value}, reused with translation only: {value2}; these are the suffixed ones that were previously messed up by zeroing)");
		if (dictionary.Count == 0)
		{
			log(" ✗ No collision pieces read from m13, make sure you selected the original Elden Ring m13 MSB.");
			return;
		}
		log("[2/4] Calculating building offset D from buildings (map tiles + assets)...");
		Vector3 d = ComputeBuildingOnlyOffset(mergedMsbPath, m13MsbPath, log);
		log($"      D = ({d.X:F2}, {d.Y:F2}, {d.Z:F2})");
		if (d.Length() < 0.01f)
		{
			log(" ⚠ D=0: if the buildings were indeed not moved, collision pieces will use the Elden Ring original positions directly (no shift compensation); otherwise make sure you selected the correct two MSBs.");
		}
		log("[3/4] Repositioning m60 collision pieces using P_m13 + (I-R)·D (restoring positions of reused pieces that were zeroed)...");
		MSB_NR mSB_NR = SoulsFile<MSB_NR>.Read(mergedMsbPath);
		var (value3, value4, _) = ApplyCollisionInstanceFix(mSB_NR, dictionary, d, log);
		log("[4/4] Writing: " + Path.GetFileName(outPath));
		mSB_NR.Write(outPath);
		log($"Done! Repositioned {value3} collision pieces (reused with rotation: {value4}). Put it into map/mapstudio/ to overwrite the original MSB, then restart Smithbox to check whether the suffixed collisions are back in place.");
	}

	private static HashSet<int> LoadParamIds(string csvPath)
	{
		HashSet<int> hashSet = new HashSet<int>();
		foreach (string item in File.ReadLines(csvPath).Skip(1))
		{
			if (!string.IsNullOrWhiteSpace(item))
			{
				int num = item.IndexOf(',');
				if (int.TryParse(((num > 0) ? item.Substring(0, num) : item).Trim(), out var result))
				{
					hashSet.Add(result);
				}
			}
		}
		return hashSet;
	}

	private static int ModelNumber(string model)
	{
		if (string.IsNullOrEmpty(model) || (model[0] != 'c' && model[0] != 'C'))
		{
			return -1;
		}
		if (!int.TryParse(model.Substring(1), out var result))
		{
			return -1;
		}
		return result;
	}

	private static (HashSet<int> all, HashSet<int> withReward) LoadNpcParamIds(string csvPath)
	{
		HashSet<int> hashSet = new HashSet<int>();
		HashSet<int> hashSet2 = new HashSet<int>();
		int num = -1;
		bool flag = true;
		foreach (string item in File.ReadLines(csvPath))
		{
			if (flag)
			{
				flag = false;
				string[] array = item.Split(',');
				for (int i = 0; i < array.Length; i++)
				{
					if (array[i].Trim() == "rewardItemLot_2")
					{
						num = i;
						break;
					}
				}
			}
			else
			{
				if (string.IsNullOrWhiteSpace(item))
				{
					continue;
				}
				string[] array2 = item.Split(',');
				if (array2.Length != 0 && int.TryParse(array2[0].Trim(), out var result))
				{
					hashSet.Add(result);
					if (num >= 0 && num < array2.Length && int.TryParse(array2[num].Trim(), out var result2) && result2 > 0)
					{
						hashSet2.Add(result);
					}
				}
			}
		}
		return (all: hashSet, withReward: hashSet2);
	}

	private static int SmallestParamWithPrefix(HashSet<int> set, int modelNum)
	{
		if (modelNum < 0)
		{
			return -1;
		}
		int num = -1;
		foreach (int item in set)
		{
			if (item / 10000 == modelNum && (num < 0 || item < num))
			{
				num = item;
			}
		}
		return num;
	}

	public static void FixNpcParams(string msbPath, string npcCsvPath, string thinkCsvPath, string outPath, bool secondPass, Action<string> log)
	{
		log("[1/3] Reading Nightreign param tables(CSV)...");
		(HashSet<int> all, HashSet<int> withReward) tuple = LoadNpcParamIds(npcCsvPath);
		HashSet<int> item = tuple.all;
		HashSet<int> item2 = tuple.withReward;
		HashSet<int> hashSet = LoadParamIds(thinkCsvPath);
		log($" NpcParam table {item.Count} entries (of which rewardItemLot_2 is not empty: {item2.Count} entries), NpcThinkParam table {hashSet.Count} entries");
		if (item.Count == 0)
		{
			log(" ✗ No IDs read from NpcParam.csv, make sure you selected the Nightreign NpcParam.csv(first column is ID, first row is the header).");
			return;
		}
		if (hashSet.Count == 0)
		{
			log(" ✗ No IDs read from NpcThinkParam.csv, make sure you selected the Nightreign NpcThinkParam.csv.");
			return;
		}
		log(secondPass ? ("[2/3] Secondary matching: reading " + Path.GetFileName(msbPath) + ", upgrading monsters without rewards to same-model variants that have rewardItemLot_2...") : ("[2/3] First matching: reading " + Path.GetFileName(msbPath) + ", matching monster params by model(preferring those with rewards)..."));
		MSB_NR mSB_NR = SoulsFile<MSB_NR>.Read(msbPath);
		int num = 0;
		int num2 = 0;
		int num3 = 0;
		int num4 = 0;
		int num5 = 0;
		int num6 = 0;
		int num7 = 0;
		int num8 = 0;
		SortedDictionary<string, (bool, int, bool, int)> sortedDictionary = new SortedDictionary<string, (bool, int, bool, int)>();
		foreach (MSB_NR.Part.Enemy enemy in mSB_NR.Parts.Enemies)
		{
			int modelNum = ModelNumber(enemy.ModelName);
			bool flag = false;
			bool flag2 = false;
			int npcParamId = enemy.NpcParamId;
			int npcThinkParamId = enemy.NpcThinkParamId;
			if (item.Contains(enemy.NpcParamId))
			{
				if (secondPass && !item2.Contains(enemy.NpcParamId))
				{
					int num9 = SmallestParamWithPrefix(item2, modelNum);
					if (num9 >= 0 && num9 != enemy.NpcParamId)
					{
						enemy.NpcParamId = num9;
						num4++;
					}
					else
					{
						num++;
					}
				}
				else
				{
					num++;
				}
			}
			else
			{
				int num10 = SmallestParamWithPrefix(item2, modelNum);
				int num11 = SmallestParamWithPrefix(item, modelNum);
				int num12 = ((num10 >= 0) ? num10 : num11);
				if (num12 >= 0)
				{
					enemy.NpcParamId = num12;
					num2++;
					if (num10 >= 0)
					{
						num3++;
					}
				}
				else
				{
					num5++;
					flag = true;
				}
			}
			if (hashSet.Contains(enemy.NpcThinkParamId))
			{
				num6++;
			}
			else
			{
				int num13 = SmallestParamWithPrefix(hashSet, modelNum);
				if (num13 >= 0)
				{
					enemy.NpcThinkParamId = num13;
					num7++;
				}
				else
				{
					num8++;
					flag2 = true;
				}
			}
			if ((flag | flag2) && enemy.ModelName != null && !sortedDictionary.ContainsKey(enemy.ModelName))
			{
				sortedDictionary[enemy.ModelName] = (flag, npcParamId, flag2, npcThinkParamId);
			}
		}
		log($" Total monsters: {mSB_NR.Parts.Enemies.Count}");
		if (secondPass)
		{
			log($" NpcParamId: upgraded to with rewards {num4}, kept {num}, matched by model {num2}(of which with rewards {num3}), no match {num5}");
		}
		else
		{
			log($" NpcParamId: kept {num}, matched by model {num2}(of which matched with rewards {num3}), no match {num5}");
		}
		log($" NpcThinkParamId: kept {num6}, matched by model {num7}, no match {num8}");
		if (sortedDictionary.Count > 0)
		{
			log($" ⚠ The following {sortedDictionary.Count} models have no corresponding param in the Nightreign table(original values kept, you need to handle them manually):");
			foreach (KeyValuePair<string, (bool, int, bool, int)> item3 in sortedDictionary)
			{
				List<string> list = new List<string>();
				if (item3.Value.Item1)
				{
					list.Add($"NpcParam={item3.Value.Item2}");
				}
				if (item3.Value.Item3)
				{
					list.Add($"Think={item3.Value.Item4}");
				}
				log($"          {item3.Key}: {string.Join(", ", list)} (Nightreign has no {ModelNumber(item3.Key)}* params, mostly Elden Ring-exclusive bosses/elites)");
			}
		}
		else
		{
			log(" ✓ All monster params now correspond to params that exist in Nightreign.");
		}
		log("[3/3] Writing: " + Path.GetFileName(outPath));
		mSB_NR.Write(outPath);
		log("Done! Put it into map/mapstudio/ to overwrite the original MSB.");
	}

	public static void AutoAlignAndMergeCollision(string addHBhd, string baseHBhd, string mergedMsbPath, string m13MsbPath, string outDir, Action<string> log)
	{
		log("=== Auto-align and merge collision ===");
		log("[Step 1/4] Reading MSB to calculate building offsets (using only map tiles + assets; collision piece positions are often zeroed and unreliable)...");
		Vector3 vector = ComputeBuildingOnlyOffset(mergedMsbPath, m13MsbPath, log);
		float x = vector.X;
		float y = vector.Y;
		float z = vector.Z;
		if (vector.Length() < 0.01f)
		{
			log(" Offset is 0, will only merge without shifting (this is normal if the buildings were not moved; otherwise confirm the buildings in m60 have been moved into place).");
		}
		string text = DeriveLowFromHigh(addHBhd);
		string text2 = DeriveLowFromHigh(baseHBhd);
		bool flag = File.Exists(text) && File.Exists(DeriveBdt2(text));
		bool flag2 = File.Exists(text2) && File.Exists(DeriveBdt2(text2));
		bool flag3 = flag & flag2;
		if (flag3)
		{
			log(" Auto-paired low collision: " + Path.GetFileName(text) + " + " + Path.GetFileName(text2));
		}
		else
		{
			log($" No paired low collision found (Elden Ring {(flag ? "present" : "none")} / Nightreign {(flag2 ? "present" : "none")}), only processing high collision.");
		}
		Directory.CreateDirectory(outDir);
		string text3 = Path.Combine(outDir, "_shifted_tmp");
		Directory.CreateDirectory(text3);
		log("[Step 2/4] Shifting Elden Ring high collision (h)...");
		CollisionShiftCore.ShiftBinder(addHBhd, DeriveBdt2(addHBhd), text3, x, y, z, log);
		string addHBhd2 = Path.Combine(text3, Path.GetFileName(addHBhd));
		string text4 = null;
		if (flag3)
		{
			log("[Step 3/4] Shifting Elden Ring low collision (l)...");
			CollisionShiftCore.ShiftBinder(text, DeriveBdt2(text), text3, x, y, z, log);
			text4 = Path.Combine(text3, Path.GetFileName(text));
		}
		else
		{
			log("[Step 3/4] No paired low collision, skipped.");
		}
		log("[Step 4/4] Merging into m60 tile...");
		string outHBhd = Path.Combine(outDir, Path.GetFileName(baseHBhd));
		string outLBhd = (flag3 ? Path.Combine(outDir, Path.GetFileName(text2)) : null);
		string remapPath = Path.Combine(outDir, "Collision remap table.txt");
		MergeMapCollision(baseHBhd, addHBhd2, flag3 ? text2 : null, flag3 ? text4 : null, outHBhd, outLBhd, remapPath, log, dropHighPieces: true);
		try
		{
			Directory.Delete(text3, recursive: true);
		}
		catch
		{
		}
		string text5 = null;
		if (MathF.Abs(x) > 0.01f || MathF.Abs(y) > 0.01f || MathF.Abs(z) > 0.01f)
		{
			try
			{
				MSB_NR mSB_NR = SoulsFile<MSB_NR>.Read(mergedMsbPath);
				Dictionary<string, (Vector3, Vector3)> m13col = ReadCollisionTransforms(m13MsbPath);
				(int fixedCount, int rotCount, int unmatched) tuple = ApplyCollisionInstanceFix(mSB_NR, m13col, new Vector3(x, y, z), log);
				int item = tuple.fixedCount;
				int item2 = tuple.rotCount;
				text5 = Path.Combine(outDir, Path.GetFileNameWithoutExtension(mergedMsbPath) + "_collision zeroed.msb.dcx");
				mSB_NR.Write(text5);
				log("");
				log($"Corrected MSB output: {Path.GetFileName(text5)} (repositioned using Elden Ring original position P_m13+(I-R)·D: {item} collision pieces, rotated reused {item2})");
			}
			catch (Exception ex)
			{
				log("");
				log("Note: tried to output a corrected MSB automatically but failed (" + ex.Message.Split('\n')[0] + ")。");
				log($" Please manually change the position of those collision pieces in the MSB with position=({x:F1},{y:F1},{z:F1}) back to (0,0,0).");
			}
		}
		log("");
		log("Done! Aligned collision has been output to: " + outDir);
		log("Usage:");
		log(" 1) Put the output h60/l60 (.hkxbhd + .hkxbdt) into the mod's map folder to overwrite the original files.");
		if (text5 != null)
		{
			log(" 2) Use the output " + Path.GetFileName(text5) + " to replace your m60 MSB (collision pieces are zeroed, avoiding double offset/crashes).");
		}
		else
		{
			log(" 2) Offset is 0, the MSB does not need changes.");
		}
	}
}
