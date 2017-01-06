using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using TraceLogAsync;

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

                string[] lstFile = Directory.GetFiles(@"C:\ProgramData\CtrlPc\LOG","*_ERREUR_*");
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
                            //string colonne1 = "";
                            //string codeappli2 = "";
                            //string statut = "";
                            //string colonne4 = "";
                            int codeerreur = 0;
                            LogWriter write = LogWriter.Instance;
                            foreach (string line in ligne)
                            {
                                if (line.Length > 5)
                                {
                                    if (line.Substring(0, 5).Contains("/"))
                                    {
                                        string[] colonne = line.Split('\t');
                                        string date = colonne[0];
                                        string appli = colonne[1];
                                        string tmp = colonne[2];
                                        string typeStatus = "";
                                        try
                                        {
                                            typeStatus = tmp.Substring(0, tmp.IndexOf(" : "));
                                        }
                                        catch (Exception err)
                                        {
                                            write.WriteToLog("Erreur sur récupération du status", 1, "RemLogWS_ERREUR");
                                            write.WriteToLog(err.Message, 1, "RemLogWS_ERREUR");
                                            write.WriteToLog(err.StackTrace, 1, "RemLogWS_ERREUR");
                                            break;
                                        }
                                        
                                        string message = "";
                                        try
                                        {
                                            message = tmp.Substring(tmp.IndexOf(" : ") + 3);
                                        }
                                        catch (Exception err)
                                        {
                                            write.WriteToLog("Erreur sur récupération du Message : ", 1, "RemLogWS_ERREUR");
                                            write.WriteToLog("Colonne 2 ="+tmp, 1, "RemLogWS_ERREUR");
                                            write.WriteToLog(err.Message, 1, "RemLogWS_ERREUR");
                                            write.WriteToLog(err.StackTrace, 1, "RemLogWS_ERREUR");
                                            break;
                                        }
                                        
                                        //colonne1 = line.Substring(0, 19);
                                        //codeappli2 = line.Substring(24, line.LastIndexOf("    ") - 24);
                                        //statut = line.Substring(line.LastIndexOf("     ") + 5, line.IndexOf(" : ", line.LastIndexOf("     ") + 5) - line.LastIndexOf("     ") - 5);
                                        //colonne4 = line.Substring(line.IndexOf(" : ", line.LastIndexOf("     ") + 5) + 3);
                                        date = date.Trim();
                                        appli = appli.Trim();
                                        typeStatus = typeStatus.Trim();
                                        if (message.Contains("'"))
                                        {
                                            message = message.Replace("'", "''");
                                        }

                                        if (typeStatus == "INFO")
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
                                            dateTraitement = Convert.ToDateTime(date);
                                            
                                            if (status == 2 && statusParam == 2)
                                            {
                                                string result = "ok";
                                                if (codeerreur == 1 && (type == 3 || type == 1))
                                                {
                                                    result = ws.TraceLogNew(Guid.ToString(), dateTraitement, "RemLogWS.exe", codeerreur, message);
                                                }
                                                if (codeerreur == 2 && (type == 3 || type == 2))
                                                {
                                                    result = ws.TraceLogNew(Guid.ToString(), dateTraitement, "RemLogWS.exe", codeerreur, message);
                                                }
                                                if (result == "RELICA")
                                                {
                                                    
                                                    try
                                                    {
                                                        write.WriteToLog(message, Convert.ToInt32(codeerreur), "RELICA");
                                                    }
                                                    catch (Exception err)
                                                    {
                                                        write.WriteToLog(message, Convert.ToInt32(codeerreur), "RELICA_ERREUR");
                                                        write.WriteToLog(err.Message, Convert.ToInt32(codeerreur), "RELICA_ERREUR");
                                                    }
                                                    
                                                }

                                            }
                                            else if (status == 3 && statusParam == 3)//mode journal
                                            {
                                                //LogWriter write = LogWriter.Instance;
                                                try
                                                {
                                                    write.WriteToLog(message, Convert.ToInt32(codeerreur), "JOURNAL");
                                                }
                                                catch (Exception err)
                                                {
                                                    write.WriteToLog(message, Convert.ToInt32(codeerreur), "JOURNAL_ERREUR");
                                                    write.WriteToLog(err.Message, Convert.ToInt32(codeerreur), "JOURNAL_ERREUR");
                                                }

                                            }
                                            else if (status == 3 && statusParam == 2) //mode relica
                                            {
                                                //LogWriter write = LogWriter.Instance;
                                                try
                                                {
                                                    if (codeerreur == 1 && (type == 3 || type == 1))
                                                    {
                                                        write.WriteToLog(message, Convert.ToInt32(codeerreur), "RELICA");
                                                        ws.SetIncrementeRelica(Guid.ToString());
                                                    }
                                                    if (codeerreur == 2 && (type == 3 || type == 2))
                                                    {
                                                        write.WriteToLog(message, Convert.ToInt32(codeerreur), "RELICA");
                                                        ws.SetIncrementeRelica(Guid.ToString());
                                                    }

                                                }
                                                catch (Exception err)
                                                {
                                                    write.WriteToLog(message, Convert.ToInt32(codeerreur), "RELICA_ERREUR");
                                                    write.WriteToLog(err.Message, Convert.ToInt32(codeerreur), "RELICA_ERREUR");
                                                }
                                            }


                                            //Console.WriteLine(dateTraitement.ToString("yyyyMMddHHmmss") + ";" + colonne2);
                                        }
                                        catch (Exception err)
                                        {
                                            //LogWriter write = LogWriter.Instance;
                                            write.WriteToLog(err.Message, Convert.ToInt32(codeerreur), "RemLogWs_ERREUR");
                                            
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
                                            //LogWriter write = LogWriter.Instance;
                                            write.WriteToLog(line, Convert.ToInt32(codeerreur), "JOURNAL");
                                        }
                                    }

                                }


                            }
                        }
                        File.Delete(file + "_transfert");
                    }
                    catch (Exception err)
                    {
                        LogWriter write = LogWriter.Instance;
                        write.WriteToLog(err.Message, 1, "RemLogWs_ERREUR");
                    }
                }
            }
        }
    }
}

