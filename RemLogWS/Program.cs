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
            int status = 3;
            int statusParam = 3;
            int type = 3;
            ReferenceWSCtrlPc.WSCtrlPc ws = new ReferenceWSCtrlPc.WSCtrlPc();
            Object Guid = Registry.GetValue(@"HKEY_USERS\.DEFAULT\Software\CtrlPc\Version", "GUID", null);
            DateTime dateTraitement = DateTime.Now;

            if (File.Exists(@"c:\ProgramData\CtrlPc\SCRIPT\RemLog.nfo"))
            {
                using (FileStream filestream = new FileStream(@"c:\ProgramData\CtrlPc\SCRIPT\RemLog.nfo", FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    using (StreamReader read = new StreamReader(filestream))
                    {
                        string ligne;
                        while ((ligne = read.ReadLine()) != null)
                        {
                            if (ligne.Length > 2)
                            {
                                string[] colonne = ligne.Split(';');
                                Int32.TryParse(colonne[0], out status);
                                Int32.TryParse(colonne[1], out statusParam);
                                Int32.TryParse(colonne[2], out type);
                            }
                        }
                    }
                }
            }

            if (status != 1)
            {

                string[] lstFile = Directory.GetFiles(@"C:\ProgramData\CtrlPc\LOG\JOURNAL_ERREUR_*");
                foreach (string file in lstFile)
                {
                    try
                    {
                        if (File.Exists(file + "_transfert"))
                        {
                            File.Delete(file + "_transfert");
                        }
                        File.Move(file, file + "_transfert");
                        if (File.Exists(file + "_transfert"))
                        {
                            string[] ligne = File.ReadAllLines(file + "_transfert");
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
                                            if (status == 2 && statusParam == 2)
                                            {

                                                if (codeerreur == 1 && (type == 3 || type == 1))
                                                {
                                                    string result = ws.TraceLogNew(Guid.ToString(), dateTraitement, "RemLogWS.exe", codeerreur, colonne4);
                                                }
                                                if (codeerreur == 2 && (type == 3 || type == 2))
                                                {
                                                    string result = ws.TraceLogNew(Guid.ToString(), dateTraitement, "RemLogWS.exe", codeerreur, colonne4);
                                                }

                                            }

                                            //Console.WriteLine(dateTraitement.ToString("yyyyMMddHHmmss") + ";" + colonne2);
                                        }
                                        catch (Exception err)
                                        {
                                            string NameDate = dateTraitement.ToString("yyyyMMdd");
                                            string Date = dateTraitement.ToString("dd/MM/yyyy HH:mm:ss");
                                            using (StreamWriter writer = new StreamWriter(@"C:\ProgramData\CtrlPc\LOG\RemLogWS_" + NameDate + ".log"))
                                            {
                                                writer.WriteLine(Date + "     " + "RemLogWS.exe" + "     " + "ERREUR : " + err.Message);
                                            }
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
                                            string NameDate = dateTraitement.ToString("yyyyMMdd");
                                            string Date = dateTraitement.ToString("dd/MM/yyyy HH:mm:ss");
                                            using (StreamWriter writer = new StreamWriter(@"C:\ProgramData\CtrlPc\LOG\RemLogWS_" + NameDate + ".log"))
                                            {
                                                writer.WriteLine(Date + "     " + "RemLogWS.exe" + "     " + "ERREUR : " + err.Message);
                                            }
                                        }
                                    }

                                }


                            }
                        }
                    }
                    catch (Exception err)
                    {
                        string NameDate = dateTraitement.ToString("yyyyMMdd");
                        string Date = dateTraitement.ToString("dd/MM/yyyy HH:mm:ss");
                        using (StreamWriter writer = new StreamWriter(@"C:\ProgramData\CtrlPc\LOG\RemLogWS_" + NameDate + ".log"))
                        {
                            writer.WriteLine(Date + "     " + "RemLogWS.exe" + "     " + "ERREUR : " + err.Message);
                        }
                    }
                }
            }
        }
    }
}

