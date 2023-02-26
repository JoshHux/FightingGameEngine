#if UNITY_EDITOR || DEVELOPMENT_BUILD

using System;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using FightingGameEngine.Data;

namespace FightingGameEngine.Commands
{
    public struct FuncInfo
    {
        public string FuncName;
        public int ParamNum;

        public FuncInfo(string nm, int pn)
        {
            this.FuncName = nm;
            this.ParamNum = pn;
        }
    }

    public static class JackScriptCompiler
    {

        private static FuncInfo[] commands = { new FuncInfo("SetHitboxes", 1), new FuncInfo("SetHurtboxes", 1), new FuncInfo("SetVel", 1) };

        public static List<FrameData> CompileString(in string str, in soStateData state)
        {
            List<ICommand> cmd = new List<ICommand>();
            List<FrameData> ret = new List<FrameData>();

            //split new line, each line means a different thing
            //https://stackoverflow.com/questions/3989816/reading-a-string-line-per-line-in-c-sharp
            var lines = str.Split(new string[] { Environment.NewLine, "\r\n", "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries);
            int i = 0;
            foreach (var line in lines)
            {
                //get rid of special characters, get to that sweet, sweet, non-white-character string!
                //https://stackoverflow.com/questions/4140723/how-to-remove-new-line-characters-from-a-string
                var ln = Regex.Replace(line, @"\t|\n|\r", "");

                //UnityEngine.Debug.Log(ln);

                //skip commented lines
                if (ln.StartsWith("#")) { continue; }
                else if (ln.StartsWith("if"))
                {
                    //https://stackoverflow.com/questions/17252615/get-string-between-two-strings-in-a-string
                    int pFrom = ln.IndexOf("(") + 1;
                    int pTo = ln.LastIndexOf(")");
                    String strParam = ln.Substring(pFrom, pTo - pFrom);

                    //the parameters of the function as a list of strings
                    var funcParams = strParam.Split(new string[] { "==", "!=" }, StringSplitOptions.None);
                    //number of parameters to debug with
                    int paramNum = funcParams.Length;

                    if (funcParams[0] == "frame")
                    {
                        //we want to create a new frame, let's get to it!

                        //we already have a new frame, let's load the events we have into it
                        if (ret.Count > 0) { ret[ret.Count - 1].SetEvents(cmd); }

                        cmd.Clear();
                        //get the int of the frame we want to trigger on
                        int f = Int32.Parse(funcParams[1]);
                        if (f > state.Duration || f <= 0) { UnityEngine.Debug.LogWarning("Line " + i + " in " + state.name + ":\nFrame will not be reached: " + f); }

                        UnityEngine.Debug.Log("added new frame");
                        ret.Add(new FrameData(f));


                    }
                    else { UnityEngine.Debug.LogError("Line " + i + " in " + state.name + ":\nParameter \"" + funcParams[0] + "\" is not found"); }



                }
                else
                {

                    if (ret.Count == 0) { UnityEngine.Debug.LogError("Line " + i + " in " + state.name + ":\nEvent is not tied to frame on: " + line); }

                    //interperate the line, get the command
                    var toAdd = JackScriptCompiler.InterperateLine(ln, state, i);
                    if (toAdd == null) { UnityEngine.Debug.LogError("Line " + i + " in " + state.name + ":\nFunction not found in: " + ln); }

                    if (toAdd is SetHitboxEvent)
                        UnityEngine.Debug.Log("added hitbox event");
                    else if (toAdd is SetHurtboxEvent)
                        UnityEngine.Debug.Log("added hurtbox event");

                    cmd.Add(toAdd);
                }
                i++;
            }

            //avoid the corner case where we're terminating when there are events that are needed to be added to the frame
            //we'll never be replacing a frame that already has events, if we are, then something has gone wrong
            if (ret[ret.Count - 1].get_events().Count == 0 && cmd.Count > 0)
            {
                ret[ret.Count - 1].SetEvents(cmd);
            }

            return ret;
        }

        private static ICommand InterperateLine(in string ln, in soStateData state, int lnNum)
        {

            //super rough way of getting parameters
            //https://stackoverflow.com/questions/17252615/get-string-between-two-strings-in-a-string
            int pFrom = ln.IndexOf("(") + 1;
            int pTo = ln.LastIndexOf(")");
            String strParam = ln.Substring(pFrom, pTo - pFrom);

            //the parameters of the function as a list of strings
            var funcParams = strParam.Split(',');
            //number of parameters to debug with
            int paramNum = funcParams.Length;

            int i = 0;
            int len = JackScriptCompiler.commands.Length;

            do
            {
                var nm = commands[i].FuncName;
                var pn = commands[i].ParamNum;

                if (ln.StartsWith(nm))
                {
                    if (paramNum != pn) { JackScriptCompiler.ThrowParamNumError(nm, state.name, lnNum, pn, paramNum); }
                    var method = typeof(CommandFactory).GetMethod(nm);
                    return (ICommand)method.Invoke(null, new object[] { funcParams, state, lnNum });
                }

                i++;

            } while (i < len);

            return null;

        }

        private static void ThrowParamNumError(in string func, in string stateName, int lnNum, int correctNum, int foundNum) { UnityEngine.Debug.LogError("Line " + lnNum + " in " + stateName + ":\nFunction " + func + " has too few or too many parameter(s), should have " + correctNum + ", found " + foundNum); }
    }
}

#endif