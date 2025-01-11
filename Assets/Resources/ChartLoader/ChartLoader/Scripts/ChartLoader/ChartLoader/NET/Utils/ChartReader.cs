using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

using ChartLoader.NET.Utils;

using ChartLoader.NET.Framework;

namespace ChartLoader.NET.Utils
{
    /// <summary>
    /// Reads and manipulates external chart files.
    /// </summary>
    public class ChartReader
    {
        private string _path;

        /// <summary>
        /// The chart file path in an external source.
        /// </summary>
        public string Path
        {
            get
            {
                return _path;
            }
        }

        private Chart _chart;
        /// <summary>
        /// The associated chart.
        /// </summary>
        public Chart Chart
        {
            get
            {
                return _chart;
            }
        }

        private string[] _fileData;

        /// <summary>
        /// The file data as a string array.
        /// </summary>
        public string[] FileData
        {
            get
            {
                return _fileData;
            }
        }

        // The file stream enumerator.
        private IEnumerator _fileScanner;

        /// <summary>
        /// Constructor.
        /// </summary>
        
        public ChartReader()
        {
            _path = "";
            _chart = new Chart();
            _fileData = new string[0];
            
        }

        /// <summary>
        /// Reads a chart file, given a specified external path, and returns the chart object.
        /// </summary>
        /// <param name="path">The chart path.</param>
        /// <returns>Chart</returns>
        public Chart ReadChartFile(string path)
        {
            _path = path;
            try
            {
                _fileData = IO.ReadFile(_path);
                if (_fileData == null || _fileData.Length == 0)
                {
                    Debug.LogError($"File data is empty or null: {_path}");
                    return null;
                }
                ParseChartText(_fileData);
                return Chart;
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to read chart file at {_path}: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Parses the current string.
        /// </summary>
        /// <param name="chartText">The string to parse.</param>
        /// <returns>Chart</returns>
        public Chart ParseChartText(string chartText)
        {
            try
            {
                string[] stringLines = chartText.Split(new[] { Environment.NewLine }, StringSplitOptions.None);
                ParseChartText(stringLines);
                return Chart;
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to parse chart text: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Parses the current string array.
        /// </summary>
        /// <param name="chartTextArray">The string array to parse.</param>
        /// <returns>Chart</returns>
        public Chart ParseChartText(string[] chartTextArray)
        {
            try
            {
                _fileData = chartTextArray;
                _fileScanner = _fileData.GetEnumerator();
                ProcessFile();
                return Chart;
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to parse chart text array: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Processes the file string array and converts it into meaningful data.
        /// </summary>
        private void ProcessFile()
        {
            Chart.Notes.Clear();
            string currentLine;
            while ((_fileScanner.MoveNext()) && (_fileScanner.Current != null))
            {
                currentLine = _fileScanner.Current as string;

                if (string.IsNullOrWhiteSpace(currentLine))
                {
                    Debug.LogWarning("Skipping empty or whitespace-only line.");
                    continue;
                }

                try
                {
                    ProcessLine(currentLine);
                }
                catch (Exception ex)
                {
                    Debug.LogError($"Error processing line: {currentLine}. Exception: {ex.Message}");
                }
            }
        }

        /// <summary>
        /// Processes the current line and parses it.
        /// </summary>
        /// <param name="line">The line to process.</param>
        private void ProcessLine(string line)
        {
            Debug.Log($"Processing line: {line}");

            switch (line)
            {
                case "[Song]":
                    _chart.ProcessEnumerator(_fileScanner);
                    break;

                case "[SyncTrack]":
                    _chart.SynchTracks = SynchTrack.ProcessSynchTracks(_fileScanner, Chart);
                    break;

                case "[Events]":
                    _chart.Sections = Section.ProcessEvents(_fileScanner, Chart);
                    break;

                default:
                    ProcessNoteEvents(line);
                    break;
            }
        }

        /// <summary>
        /// Processes all note events.
        /// </summary>
        /// <param name="NoteType">The current note type.</param>
        internal void ProcessNoteEvents(string NoteType)
        {
            Dictionary<string, Array> container;
            Note[] playerNotes;
            StarPower[] playerSP;

            string currentLine;
            NoteEvent noteEvent = null;
            NoteEvent previousNoteEvent = null;

            List<Note> notesList = new List<Note>();
            List<StarPower> starPowersList = new List<StarPower>();

            NoteType = NoteType.Replace("[", string.Empty).Replace("]", string.Empty);

            while ((_fileScanner.MoveNext()) && (_fileScanner.Current != null))
            {
                currentLine = _fileScanner.Current as string;

                if (string.IsNullOrWhiteSpace(currentLine))
                {
                    Debug.LogWarning("Skipping empty or whitespace-only line in notes section.");
                    continue;
                }

                if (currentLine.Contains("}"))
                    break;

                if (!currentLine.Contains("{"))
                {
                    try
                    {
                        CheckLineContent(currentLine, ref noteEvent, ref previousNoteEvent, ref notesList, ref starPowersList, NoteType);
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError($"Error processing note line: {currentLine}. Exception: {ex.Message}");
                    }
                }
            }

            container = new Dictionary<string, Array>();

            playerNotes = notesList.ToArray();
            playerSP = starPowersList.ToArray();

            container.Add("Notes", playerNotes);
            container.Add("SP", playerSP);

            if (!Chart.Notes.ContainsKey(NoteType))
            {
                Chart.Notes.Add(NoteType, container);
            }
            else
            {
                Debug.LogWarning($"Duplicate note type found: {NoteType}. Ignoring.");
            }
        }

        /// <summary>
        /// Checks the current file line, and compares the contents with the previous event.
        /// </summary>
        internal void CheckLineContent(string line, ref NoteEvent currentEvent, ref NoteEvent previousEvent,
            ref List<Note> notesList, ref List<StarPower> starPowersList, string keyParent)
        {
            EventLine eventLine = new EventLine(line);

            // Create a new NoteEvent and process the provided line
            currentEvent = new NoteEvent(Chart, eventLine, keyParent);

            if (eventLine.Index == 5)
                currentEvent.ForcedSolid = true;

            if (eventLine.Index == 6)
                currentEvent.IsHOPO = true;

            if (notesList.Count == 0)
                notesList.Add(Note.GetCopy(currentEvent));

            if (previousEvent == null)
                previousEvent = (NoteEvent)currentEvent.Clone();

            if (previousEvent.Tick == currentEvent.Tick && previousEvent.Type == currentEvent.Type)
            {
                previousEvent.AppendFret(eventLine);
                if (previousEvent.Type.Contains("N"))
                {
                    if (notesList.Count > 0)
                        notesList[notesList.Count - 1] = Note.GetCopy(previousEvent);
                }
                else
                {
                    if (starPowersList.Count > 0)
                        starPowersList[starPowersList.Count - 1] = StarPower.GetCopy(previousEvent);
                }
            }
            else
            {
                if (currentEvent.Type.Contains("N"))
                {
                    currentEvent.Index = notesList.Count;
                    notesList.Add(Note.GetCopy(currentEvent));
                }
                else
                {
                    currentEvent.Index = starPowersList.Count;
                    starPowersList.Add(StarPower.GetCopy(currentEvent));
                }

                previousEvent = currentEvent;
            }
        }
    }
}
