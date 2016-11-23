using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace RemLogWS
{
    class Program
    {
        static void Main(string[] args)
        {
            ReferenceWSCtrlPc.WSCtrlPc ws = new ReferenceWSCtrlPc.WSCtrlPc();
            string pathJournal = @"C:\ProgramData\CtrlPc\LOG\Journal.log";
            Object Guid = Registry.GetValue(@"HKEY_USERS\.DEFAULT\Software\CtrlPc\Version", "GUID", null);
            string pathJournalTransfert = @"C:\ProgramData\CtrlPc\LOG\Journal_transfert.log";
            DateTime dateTraitement = DateTime.Now;
            if (File.Exists(pathJournal))
            {
                try
                {
                    if (File.Exists(pathJournalTransfert))
                    {
                        File.Delete(pathJournalTransfert);
                    }
                    File.Move(pathJournal, pathJournalTransfert);
                    if (File.Exists(pathJournalTransfert))
                    {
                        string[] ligne = File.ReadAllLines(pathJournalTransfert);
                        string colonne1 = "";
                        string codeappli2 = "";
                        string statut = "";
                        string colonne4 = "";
                        int codeerreur = 0;
                        foreach (string line in ligne)
                        {
                            if (line.Length > 5)
                            {
                                if (line.Substring(0, 5).Contains("/"))
                                {
                                    colonne1 = line.Substring(0, 19);
                                    codeappli2 = line.Substring(24, line.LastIndexOf("    ") - 24);
                                    statut = line.Substring(line.LastIndexOf("     ") + 5, line.IndexOf(" : ", line.LastIndexOf("     ") + 5) - line.LastIndexOf("     ") - 5);
                                    colonne4 = line.Substring(line.IndexOf(" : ", line.LastIndexOf("     ") + 5) + 3);
                                    colonne1 = colonne1.Trim();
                                    codeappli2 = codeappli2.Trim();
                                    statut = statut.Trim();
                                    if (colonne4.Contains("'"))
                                    {
                                        colonne4 = colonne4.Replace("'", "''");
                                    }

                                    if (statut == "INFO")
                                    {
                                        codeerreur = 2;
                                    }
                                    else
                                    {
                                        codeerreur = 1;
                                    }
                                    // Console.WriteLine(colonne4);
                                    try
                                    {
                                        dateTraitement = Convert.ToDateTime(colonne1);
                                        ws.TraceLog(Guid.ToString(), dateTraitement, "RemLogWS.exe", codeerreur, colonne4);
                                        //Console.WriteLine(dateTraitement.ToString("yyyyMMddHHmmss") + ";" + colonne2);
                                    }
                                    catch (Exception)
                                    {
                                        ws.TraceLog(Guid.ToString(), dateTraitement, "RemLogWS.exe", codeerreur, line);
                                        //Console.WriteLine(dateTraitement.ToString("yyyyMMddHHmmss")+";"+line);
                                    }
                                }
                                else
                                {
                                    try
                                    {
                                        ws.TraceLog(Guid.ToString(), dateTraitement, "RemLogWS.exe", 1, line);
                                    }
                                    catch (Exception err)
                                    {
                                        
                                    }
                                }

                            }
                            //Console.WriteLine(colonne1);
                        }
                    }
                    File.Delete(pathJournalTransfert);
                }
                catch (Exception err)
                {
                    Console.WriteLine(err.Message);
                }
            }

        }
    }
}
