﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AxeSoftware.Quest.Functions;

namespace AxeSoftware.Quest.Scripts
{
    public class SwitchScriptConstructor : IScriptConstructor
    {
        #region IScriptConstructor Members

        public string Keyword
        {
            get { return "switch"; }
        }

        public IScript Create(string script, Element proc)
        {
            string afterExpr;
            string param = Utility.GetParameter(script, out afterExpr);
            IScript defaultScript;
            Dictionary<IFunctionGeneric, IScript> cases = ProcessCases(Utility.GetScript(afterExpr), out defaultScript, proc);

            return new SwitchScript(WorldModel, new ExpressionGeneric(param, WorldModel), cases, defaultScript);
        }

        public IScriptFactory ScriptFactory { get; set; }

        public WorldModel WorldModel { get; set; }

        #endregion

        private Dictionary<IFunctionGeneric, IScript> ProcessCases(string cases, out IScript defaultScript, Element proc)
        {
            bool finished = false;
            string remainingCases;
            string afterExpr;
            Dictionary<IFunctionGeneric, IScript> result = new Dictionary<IFunctionGeneric, IScript>();
            defaultScript = null;

            cases = Utility.RemoveSurroundingBraces(cases);

            while (!finished)
            {
                cases = Utility.GetScript(cases, out remainingCases);
                if (cases != null) cases = cases.Trim();

                if (!string.IsNullOrEmpty(cases))
                {
                    if (cases.StartsWith("case"))
                    {
                        string expr = Utility.GetParameter(cases, out afterExpr);
                        string caseScript = Utility.GetScript(afterExpr);
                        IScript script = ScriptFactory.CreateScript(caseScript, proc);

                        result.Add(new ExpressionGeneric(expr, WorldModel), script);
                    }
                    else if (cases.StartsWith("default"))
                    {
                        defaultScript = ScriptFactory.CreateScript(cases.Substring(8).Trim());
                    }
                    else
                    {
                        throw new Exception(string.Format("Invalid inside switch block: '{0}'", cases));
                    }
                }

                cases = remainingCases;
                if (string.IsNullOrEmpty(cases)) finished = true;
            }

            return result;
        }
    }

    public class SwitchScript : ScriptBase
    {
        private IFunctionGeneric m_expr;
        private SwitchCases m_cases;
        private IScript m_default;
        private WorldModel m_worldModel;

        public SwitchScript(WorldModel worldModel, IFunctionGeneric expression, Dictionary<IFunctionGeneric, IScript> cases, IScript defaultScript)
        {
            m_worldModel = worldModel;
            m_expr = expression;
            m_cases = new SwitchCases(this, cases);
            m_default = defaultScript;
        }

        public override void Execute(Context c)
        {
            object result = m_expr.Execute(c);
            bool success = false;

            // using .ToString() here as an object comparison of ints won't work
            success = m_cases.Execute(c, result.ToString());

            if (!success && m_default != null)
            {
                m_default.Execute(c);
            }
        }

        public override string Save()
        {
            string result = SaveScript("switch", m_expr.Save()) + " {" + Environment.NewLine;
            result += m_cases.Save();
            if (m_default != null) result += SaveScript("default", m_default);
            result += Environment.NewLine + "}";
            return result;
        }

        public override string Keyword
        {
            get
            {
                return "switch";
            }
        }

        public override object GetParameter(int index)
        {
            switch (index)
            {
                case 0:
                    return m_expr.Save();
                case 1:
                    return m_cases.CasesAsQuestDictionary;
                case 2:
                    return m_default;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public override void SetParameterInternal(int index, object value)
        {
            switch (index)
            {
                case 0:
                    m_expr = new ExpressionGeneric((string)value, m_worldModel);
                    break;
                case 1:
                    // any updates to the cases should change the scriptdictionary itself - nothing should cause SetParameter to be triggered.
                    throw new InvalidOperationException("Attempt to use SetParameter to change the cases of a 'switch'");
                case 2:
                    // any updates to the script should change the script itself - nothing should cause SetParameter to be triggered.
                    throw new InvalidOperationException("Attempt to use SetParameter to change the script of a 'switch'");
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        // We store the switch cases internally as a QuestDictionary<IScript>, so we can edit them in the Editor
        // using the standard scriptdictionary editor control.
        private class SwitchCases
        {
            private QuestDictionary<IScript> m_cases = new QuestDictionary<IScript>();
            private Dictionary<string, IFunctionGeneric> m_compiledExpressions = new Dictionary<string, IFunctionGeneric>();
            private SwitchScript m_parent;

            public SwitchCases(SwitchScript parent, Dictionary<IFunctionGeneric, IScript> cases)
            {
                m_parent = parent;
                m_cases.UndoLog = parent.m_worldModel.UndoLogger;

                foreach (var switchCase in cases)
                {
                    IFunctionGeneric compiledExpression = switchCase.Key;
                    string caseString = compiledExpression.Save();
                    IScript script = switchCase.Value;

                    m_cases.Add(caseString, script);
                    m_compiledExpressions.Add(caseString, compiledExpression);
                }
            }

            public QuestDictionary<IScript> CasesAsQuestDictionary
            {
                get { return m_cases; }
            }

            public string Save()
            {
                string result = string.Empty;
                foreach (KeyValuePair<string, IScript> caseItem in m_cases)
                {
                    result += m_parent.SaveScript("case", caseItem.Value, caseItem.Key);
                }
                return result;
            }

            public bool Execute(Context c, string result)
            {
                foreach (var switchCase in m_cases)
                {
                    IFunctionGeneric expr = m_compiledExpressions[switchCase.Key];

                    if (result == expr.Execute(c).ToString())
                    {
                        switchCase.Value.Execute(c);
                        return true;
                    }
                }
                return false;
            }
        }
    }
}