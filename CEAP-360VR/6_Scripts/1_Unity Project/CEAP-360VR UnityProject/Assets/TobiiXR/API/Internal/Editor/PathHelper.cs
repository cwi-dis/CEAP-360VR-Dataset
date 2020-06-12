// Copyright � 2018 � Property of Tobii AB (publ) - All Rights Reserved

namespace Tobii.XR.Internal
{
	using System;
	using System.IO;
	using System.Linq;
	using UnityEngine;

    public static class PathHelper
    {
		public static string PathCombine(params string[] paths)
		{
			return paths.Aggregate((acc, p) => Path.Combine(acc, p));
		}

		public static string FindPathToClass(Type type)
		{
			var filename = type.Name + ".cs";
			return FindAFileRecursively(Application.dataPath, filename);
		}

		private static string FindAFileRecursively(string startDir, string filename)
		{
			foreach(var file in Directory.GetFiles(startDir))
			{
				if(Path.GetFileName(file).Equals(filename)) return file;
			}

			foreach(var dir in Directory.GetDirectories(startDir))
			{
				var file = FindAFileRecursively(dir, filename);
				if(file != null) return file;
			}

			return null;
		}
    }
}