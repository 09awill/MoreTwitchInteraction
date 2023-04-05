using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MoreTwitchInteraction
{
    public static class Utils
    {
        //This code is "Borrowed from IcedMilo https://github.com/UrFriendKen/PlateUpCustomDifficulty/blob/master/Main.cs
        public static int[] GenerateIntArray(string input, out string[] stringRepresentation, int[] addValuesBefore = null, int[] addValuesAfter = null, string prefix = "", string postfix = "")
        {
            List<string> stringOutput = new List<string>();
            List<int> output = new List<int>();
            string[] ranges = input.Split(',');
            foreach (string range in ranges)
            {
                string[] extents = range.Split('|');
                int min = Convert.ToInt32(extents[0]);
                int max;
                int step;
                switch (extents.Length)
                {
                    case 1:
                        output.Add(min);
                        stringOutput.Add($"{prefix}{min}{postfix}");
                        continue;
                    case 2:
                        max = Convert.ToInt32(extents[1]);
                        step = 1;
                        break;
                    case 3:
                        max = Convert.ToInt32(extents[1]);
                        step = Convert.ToInt32(extents[2]);
                        break;
                    default:
                        continue;
                }
                for (int i = min; i <= max; i += step)
                {
                    output.Add(i);
                    stringOutput.Add($"{prefix}{i}{postfix}");
                }
            }
            stringRepresentation = stringOutput.ToArray();
            if (addValuesBefore == null)
                addValuesBefore = new int[0];
            if (addValuesAfter == null)
                addValuesAfter = new int[0];
            return addValuesBefore.AddRangeToArray(output.ToArray()).AddRangeToArray(addValuesAfter);
        }
        public static float[] GenerateFloatArray(string input, out string[] stringRepresentation, float[] addValuesBefore = null, float[] addValuesAfter = null, string prefix = "", string postfix = "")
        {
            List<string> stringOutput = new List<string>();
            List<float> output = new List<float>();
            string[] ranges = input.Split(',');
            foreach (string range in ranges)
            {
                string[] extents = range.Split('|');
                float min = Convert.ToInt32(extents[0]);
                float max;
                float step;
                switch (extents.Length)
                {
                    case 1:
                        output.Add(min);
                        stringOutput.Add($"{prefix}{min}{postfix}");
                        continue;
                    case 2:
                        max = float.Parse(extents[1]);
                        step = 1;
                        break;
                    case 3:
                        max = float.Parse(extents[1]);
                        step = float.Parse(extents[2]);
                        break;
                    default:
                        continue;
                }
                for (float i = min; i <= max; i += step)
                {
                    output.Add(i);
                    stringOutput.Add($"{prefix}{i}{postfix}");
                }
            }
            stringRepresentation = stringOutput.ToArray();
            if (addValuesBefore == null)
                addValuesBefore = new float[0];
            if (addValuesAfter == null)
                addValuesAfter = new float[0];
            return addValuesBefore.AddRangeToArray(output.ToArray()).AddRangeToArray(addValuesAfter);
        }
    }
}
