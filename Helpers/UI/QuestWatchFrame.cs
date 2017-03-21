/*
  This file is part of PPather.

    PPather is free software: you can redistribute it and/or modify
    it under the terms of the GNU Lesser General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    PPather is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU Lesser General Public License for more details.

    You should have received a copy of the GNU Lesser General Public License
    along with PPather.  If not, see <http://www.gnu.org/licenses/>.

*/

using System;
using System.Collections.Generic;
using System.Text;
using Glider.Common.Objects;
using System.Text.RegularExpressions;

namespace Pather.Helpers.UI
{
    public class QuestWatchFrame
    {
        private static GInterfaceObject frame;
        private static string[] questLines;
        private static GSpellTimer updateTimer;
        private static Regex regex;
        private static Dictionary<string, bool> questStatus;

        private static void setFrame()
        {
            if (frame == null)
            {
                frame = GContext.Main.Interface.GetByName("QuestWatchFrame");
                questLines = new string[32];
                updateTimer = new GSpellTimer(5000);
                regex = new Regex("[0-9]{1,2}\\/[0-9]{1,2}$"); //matching a/b at the end of the string where a and b are integers with 1 or 2 digits
            }
            questStatus = new Dictionary<string, bool>();
        }

        private static void UpdateQuestStatus()
        {
            setFrame();
            for (int i = 1; i < frame.Children.Length; i++)
            {
                questLines[i - 1] = frame.Children[i].LabelText;
            }

            bool completed = false;

            List<string> alreadyChecked = new List<string>();
            
            for (int i = 0; i < questLines.Length; i++)
            {
                string currentLine = questLines[i];
                if (currentLine.Contains("(no text)"))
                {
                    break;
                }
                string QuestName = currentLine;
                
                completed = true;
                i++;
                while (questLines[i].StartsWith(" - "))
                {
                    
                    currentLine = questLines[i];
                    i++;
                    if (!regex.IsMatch(currentLine))
                    {
                        completed = false;
                        break;
                    }
                    if (HasStringMatch(currentLine, alreadyChecked))
                    {
                        continue;
                    }
                    int totalCountIndex = 0;
                    int currentCountIndex = 0;

                    totalCountIndex = currentLine.LastIndexOf("/");
                    currentCountIndex = currentLine.LastIndexOf(" ");

                    string currentCountString = currentLine.Substring(currentCountIndex, totalCountIndex - currentCountIndex);
                    string totalCountString = currentLine.Substring(totalCountIndex + 1);

                    int currentCount = int.Parse(currentCountString);
                    int totalCount = int.Parse(totalCountString);

                    if (currentCount != totalCount)
                    {
                        completed = false;
                        break;
                    }

                    alreadyChecked.Add(currentLine.Substring(0, currentCountIndex));
                }
                i--;
                if(!questStatus.ContainsKey(QuestName))
                    questStatus.Add(QuestName, completed);
            }
        }

        public static bool GetQuestStatus(string name)
        {
            if (updateTimer == null || updateTimer.IsReady)
            {
                UpdateQuestStatus();
                updateTimer.Reset();
            }

            bool completed = false;

            if (!questStatus.TryGetValue(name, out completed))
            {
                return false;
            }
            else
            {
                return completed;
            }
        }

        private static bool HasStringMatch(string s, List<string> list)
        {
            foreach (string str in list)
            {
                if (s.Contains(str))
                {
                    return true;
                }
            }
            return false;
        }

    }
}
