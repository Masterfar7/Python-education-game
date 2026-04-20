using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;

public class InterpreterEngine
{
    public Dictionary<string, object> variables = new Dictionary<string, object>();
    public string lastPrintedValue = "";

    public InterpreterEngine Clone()
    {
        var copy = new InterpreterEngine();
        copy.lastPrintedValue = lastPrintedValue;
        foreach (var kv in variables)
        {
            if (kv.Value is List<object> lst)
                copy.variables[kv.Key] = new List<object>(lst);
            else
                copy.variables[kv.Key] = kv.Value;
        }

        return copy;
    }

    public bool Execute(string code)
    {
        if (string.IsNullOrWhiteSpace(code))
            return false;

        List<string> rawLines = SplitCodeLines(code);
        bool anyExecuted = false;
        int i = 0;

        while (i < rawLines.Count)
        {
            string line = rawLines[i];
            string stripped = StripComment(line).TrimEnd();
            if (string.IsNullOrWhiteSpace(stripped))
            {
                i++;
                continue;
            }

            string trimmed = stripped.TrimStart();

            if (trimmed.StartsWith("if ", System.StringComparison.Ordinal))
            {
                Match m = Regex.Match(trimmed, @"^if\s+(.+):$");
                if (!m.Success)
                    return false;

                int ifIndent = GetIndent(line);
                bool cond = EvaluateBoolExpr(m.Groups[1].Value.Trim());

                if (cond)
                {
                    i++;
                    while (i < rawLines.Count)
                    {
                        string bl = rawLines[i];
                        string bc = StripComment(bl).TrimEnd();
                        if (string.IsNullOrWhiteSpace(bc))
                        {
                            i++;
                            continue;
                        }

                        int ind = GetIndent(bl);
                        string bt = bc.TrimStart();

                        if (ind == ifIndent && bt.StartsWith("else:", System.StringComparison.Ordinal))
                        {
                            i = SkipElseBranch(rawLines, i, ifIndent);
                            break;
                        }

                        if (ind < ifIndent)
                            break;

                        if (ind == ifIndent && !bt.StartsWith("else:", System.StringComparison.Ordinal))
                            break;

                        if (ind > ifIndent)
                        {
                            if (!ExecuteSingle(bc.Trim()))
                                return false;
                            anyExecuted = true;
                        }

                        i++;
                    }
                }
                else
                {
                    i++;
                    while (i < rawLines.Count)
                    {
                        string bl = rawLines[i];
                        string bc = StripComment(bl).TrimEnd();
                        if (string.IsNullOrWhiteSpace(bc))
                        {
                            i++;
                            continue;
                        }

                        int ind = GetIndent(bl);
                        string bt = bc.TrimStart();

                        if (ind == ifIndent && bt.StartsWith("else:", System.StringComparison.Ordinal))
                        {
                            i++;
                            while (i < rawLines.Count)
                            {
                                string el = rawLines[i];
                                string ec = StripComment(el).TrimEnd();
                                if (string.IsNullOrWhiteSpace(ec))
                                {
                                    i++;
                                    continue;
                                }

                                if (GetIndent(el) <= ifIndent)
                                    break;

                                if (!ExecuteSingle(ec.Trim()))
                                    return false;
                                anyExecuted = true;
                                i++;
                            }

                            break;
                        }

                        if (ind <= ifIndent)
                            break;

                        i++;
                    }
                }

                continue;
            }

            if (trimmed.StartsWith("else:", System.StringComparison.Ordinal))
            {
                i++;
                continue;
            }

            // Обработка циклов while
            if (trimmed.StartsWith("while ", System.StringComparison.Ordinal))
            {
                Match m = Regex.Match(trimmed, @"^while\s+(.+):$");
                if (!m.Success)
                    return false;

                int whileIndent = GetIndent(line);
                int whileStartLine = i;
                string condition = m.Groups[1].Value.Trim();

                // Защита от бесконечных циклов
                int maxIterations = 10000;
                int iterations = 0;

                while (EvaluateBoolExpr(condition))
                {
                    iterations++;
                    if (iterations > maxIterations)
                    {
                        Debug.LogError("InterpreterEngine: Превышен лимит итераций цикла while (10000)");
                        return false;
                    }

                    // Выполняем тело цикла
                    int j = whileStartLine + 1;
                    while (j < rawLines.Count)
                    {
                        string bl = rawLines[j];
                        string bc = StripComment(bl).TrimEnd();
                        if (string.IsNullOrWhiteSpace(bc))
                        {
                            j++;
                            continue;
                        }

                        int ind = GetIndent(bl);

                        // Если отступ меньше или равен отступу while - выходим из тела
                        if (ind <= whileIndent)
                            break;

                        // Выполняем строку внутри цикла
                        if (ind > whileIndent)
                        {
                            if (!ExecuteSingle(bc.Trim()))
                                return false;
                            anyExecuted = true;
                        }

                        j++;
                    }
                }

                // Пропускаем тело цикла после завершения
                i++;
                while (i < rawLines.Count)
                {
                    string bl = rawLines[i];
                    string bc = StripComment(bl).TrimEnd();
                    if (string.IsNullOrWhiteSpace(bc))
                    {
                        i++;
                        continue;
                    }

                    int ind = GetIndent(bl);
                    if (ind <= whileIndent)
                        break;

                    i++;
                }

                continue;
            }

            if (trimmed.StartsWith("for ", System.StringComparison.Ordinal))
            {
                int forIndent = GetIndent(line);
                int j = i + 1;
                while (j < rawLines.Count)
                {
                    string bl = rawLines[j];
                    string bc = StripComment(bl).TrimEnd();
                    if (string.IsNullOrWhiteSpace(bc))
                    {
                        j++;
                        continue;
                    }

                    if (GetIndent(bl) <= forIndent)
                        break;
                    j++;
                }

                int bodyStart = i + 1;
                int bodyEnd = j - 1;
                string innerCode = BuildDedentBlock(rawLines, bodyStart, bodyEnd);
                if (innerCode == null)
                    return false;

                Match rangeFm = Regex.Match(trimmed, @"^for\s+(\w+)\s+in\s+range\s*\(\s*([^)]*)\s*\)\s*:\s*$");
                if (rangeFm.Success)
                {
                    string loopVar = rangeFm.Groups[1].Value;
                    string rangeArg = rangeFm.Groups[2].Value.Trim();
                    int iterCount = ParseLoopCount(rangeArg);
                    if (iterCount < 0)
                        return false;
                    iterCount = Mathf.Min(iterCount, 10000);

                    if (iterCount == 0)
                    {
                        i = j;
                        continue;
                    }

                    if (string.IsNullOrWhiteSpace(innerCode))
                    {
                        variables[loopVar] = (float)(iterCount - 1);
                        anyExecuted = true;
                        i = j;
                        continue;
                    }

                    for (int iter = 0; iter < iterCount; iter++)
                    {
                        variables[loopVar] = (float)iter;
                        if (!Execute(innerCode))
                            return false;
                        anyExecuted = true;
                    }

                    i = j;
                    continue;
                }

                Match listFm = Regex.Match(trimmed, @"^for\s+(\w+)\s+in\s+(\w+)\s*:\s*$");
                if (!listFm.Success)
                    return false;

                string loopVarName = listFm.Groups[1].Value;
                string iterableName = listFm.Groups[2].Value;
                if (!variables.TryGetValue(iterableName, out object iterObj) || !(iterObj is List<object> itemList))
                    return false;

                int n = Mathf.Min(itemList.Count, 10000);
                if (n == 0)
                {
                    i = j;
                    continue;
                }

                if (string.IsNullOrWhiteSpace(innerCode))
                {
                    variables[loopVarName] = itemList[n - 1];
                    anyExecuted = true;
                    i = j;
                    continue;
                }

                for (int idx = 0; idx < n; idx++)
                {
                    variables[loopVarName] = itemList[idx];
                    if (!Execute(innerCode))
                        return false;
                    anyExecuted = true;
                }

                i = j;
                continue;
            }

            foreach (string statement in SplitStatements(stripped + "\n"))
            {
                string s = StripComment(statement).Trim();
                if (string.IsNullOrEmpty(s))
                    continue;

                if (!ExecuteSingle(s))
                    return false;

                anyExecuted = true;
            }

            i++;
        }

        return anyExecuted;
    }

    System.Collections.Generic.IEnumerable<string> SplitStatements(string code)
    {
        var sb = new System.Text.StringBuilder();

        bool inSingle = false;
        bool inDouble = false;
        bool escape = false;
        int parenDepth = 0;
        int bracketDepth = 0;

        for (int k = 0; k < code.Length; k++)
        {
            char c = code[k];

            if (escape)
            {
                sb.Append(c);
                escape = false;
                continue;
            }

            if (c == '\\')
            {
                sb.Append(c);
                escape = true;
                continue;
            }

            if (!inDouble && c == '\'')
            {
                inSingle = !inSingle;
                sb.Append(c);
                continue;
            }

            if (!inSingle && c == '"')
            {
                inDouble = !inDouble;
                sb.Append(c);
                continue;
            }

            if (!inSingle && !inDouble)
            {
                if (c == '(') parenDepth++;
                else if (c == ')' && parenDepth > 0) parenDepth--;
                else if (c == '[') bracketDepth++;
                else if (c == ']' && bracketDepth > 0) bracketDepth--;

                bool isSeparator =
                    c == '\n' ||
                    c == '\r' ||
                    c == ';' ||
                    (c == ',' && parenDepth == 0 && bracketDepth == 0);

                if (isSeparator)
                {
                    string stmt = sb.ToString().Trim();
                    sb.Length = 0;
                    if (!string.IsNullOrEmpty(stmt))
                        yield return stmt;
                    continue;
                }
            }

            sb.Append(c);
        }

        string last = sb.ToString().Trim();
        if (!string.IsNullOrEmpty(last))
            yield return last;
    }

    static List<string> SplitCodeLines(string code)
    {
        var lines = new List<string>();
        string[] parts = code.Split(new[] { '\r', '\n' }, System.StringSplitOptions.None);
        foreach (string p in parts)
            lines.Add(p);
        return lines;
    }

    static int GetIndent(string line)
    {
        int count = 0;
        foreach (char c in line)
        {
            if (c == ' ')
                count++;
            else if (c == '\t')
                count += 4;
            else
                break;
        }

        return count;
    }

    static int SkipElseBranch(List<string> lines, int elseLineIndex, int ifIndent)
    {
        int j = elseLineIndex + 1;
        while (j < lines.Count)
        {
            string line = lines[j];
            string c = StripCommentStatic(line).TrimEnd();
            if (string.IsNullOrWhiteSpace(c))
            {
                j++;
                continue;
            }

            if (GetIndent(line) <= ifIndent)
                break;
            j++;
        }

        return j;
    }

    static string StripCommentStatic(string line)
    {
        bool inSingle = false;
        bool inDouble = false;
        bool escape = false;

        for (int i = 0; i < line.Length; i++)
        {
            char ch = line[i];

            if (escape)
            {
                escape = false;
                continue;
            }

            if (ch == '\\')
            {
                escape = true;
                continue;
            }

            if (!inDouble && ch == '\'') inSingle = !inSingle;
            else if (!inSingle && ch == '"') inDouble = !inDouble;

            if (!inSingle && !inDouble && ch == '#')
                return line.Substring(0, i);
        }

        return line;
    }

    bool ExecuteSingle(string codeLine)
    {
        Match printMatch = Regex.Match(codeLine, @"^print\s*\((.*)\)$");
        if (printMatch.Success)
        {
            string inside = printMatch.Groups[1].Value.Trim();
            List<string> argStrings = SplitTopLevelComma(inside);
            if (argStrings.Count == 1)
            {
                lastPrintedValue = EvaluatePrintArg(argStrings[0].Trim());
            }
            else
            {
                var parts = new List<string>(argStrings.Count);
                foreach (string a in argStrings)
                {
                    string t = a.Trim();
                    if (t.Length > 0)
                        parts.Add(EvaluatePrintArg(t));
                }

                lastPrintedValue = string.Join(" ", parts);
            }

            return true;
        }

        Match assignMatch = Regex.Match(codeLine, @"^(\w+)\s*=\s*(.+)$");
        if (assignMatch.Success)
        {
            string varName = assignMatch.Groups[1].Value;
            string valueExpression = assignMatch.Groups[2].Value;

            object value = EvaluateRaw(valueExpression);
            variables[varName] = value;
            return true;
        }

        return false;
    }

    string EvaluatePrintArg(string expr)
    {
        expr = expr.Trim();
        if (expr.Length == 0)
            return "";

        Match strMatch = Regex.Match(expr, @"^['""](.*)['""]$");
        if (strMatch.Success)
            return strMatch.Groups[1].Value;

        if (variables.TryGetValue(expr, out object v))
            return ValueToPrintString(v);

        if (TryParseNumber(expr, out float num))
        {
            if (Mathf.Approximately(num, Mathf.Round(num)))
                return ((int)num).ToString(CultureInfo.InvariantCulture);
            return num.ToString(CultureInfo.InvariantCulture);
        }

        if (expr == "True" || expr == "False")
            return expr;

        return expr;
    }

    static string ValueToPrintString(object v)
    {
        if (v == null)
            return "";
        if (v is List<object>)
            return "[...]";
        if (v is float f)
        {
            if (Mathf.Approximately(f, Mathf.Round(f)))
                return ((int)f).ToString(CultureInfo.InvariantCulture);
            return f.ToString(CultureInfo.InvariantCulture);
        }

        return v.ToString();
    }

    static List<string> SplitTopLevelComma(string s)
    {
        var result = new List<string>();
        var sb = new StringBuilder();
        int parenDepth = 0;
        int bracketDepth = 0;
        bool inSingle = false;
        bool inDouble = false;
        bool escape = false;

        for (int i = 0; i < s.Length; i++)
        {
            char c = s[i];

            if (escape)
            {
                sb.Append(c);
                escape = false;
                continue;
            }

            if ((inSingle || inDouble) && c == '\\')
            {
                sb.Append(c);
                escape = true;
                continue;
            }

            if (!inDouble && c == '\'')
            {
                inSingle = !inSingle;
                sb.Append(c);
                continue;
            }

            if (!inSingle && c == '"')
            {
                inDouble = !inDouble;
                sb.Append(c);
                continue;
            }

            if (!inSingle && !inDouble)
            {
                if (c == '(') parenDepth++;
                else if (c == ')' && parenDepth > 0) parenDepth--;
                else if (c == '[') bracketDepth++;
                else if (c == ']' && bracketDepth > 0) bracketDepth--;

                if (c == ',' && parenDepth == 0 && bracketDepth == 0)
                {
                    result.Add(sb.ToString());
                    sb.Length = 0;
                    continue;
                }
            }

            sb.Append(c);
        }

        result.Add(sb.ToString());
        return result;
    }

    object EvaluateRaw(string expr)
    {
        expr = expr.Trim();

        if (expr.Length >= 2 && expr[0] == '[' && expr[expr.Length - 1] == ']')
            return ParseListLiteral(expr.Substring(1, expr.Length - 2));

        if (LooksLikeBoolExpression(expr))
            return EvaluateBoolExpr(expr);

        int mulIndex = expr.IndexOf('*');
        if (mulIndex > 0)
        {
            string left = expr.Substring(0, mulIndex).Trim();
            string right = expr.Substring(mulIndex + 1).Trim();

            float leftVal = ResolveNumberOrVariable(left);
            float rightVal = ResolveNumberOrVariable(right);

            return leftVal * rightVal;
        }

        Match strMatch = Regex.Match(expr, @"['""](.*?)['""]");
        if (strMatch.Success)
            return strMatch.Groups[1].Value;

        if (TryParseNumber(expr, out float number))
            return number;

        if (expr == "True") return true;
        if (expr == "False") return false;

        if (variables.ContainsKey(expr))
            return variables[expr];

        return expr;
    }

    List<object> ParseListLiteral(string inner)
    {
        inner = inner.Trim();
        var list = new List<object>();
        if (inner.Length == 0)
            return list;

        foreach (string part in SplitTopLevelComma(inner))
        {
            string p = part.Trim();
            if (p.Length == 0)
                continue;
            list.Add(EvaluateRaw(p));
        }

        return list;
    }

    static bool LooksLikeBoolExpression(string expr)
    {
        return expr.IndexOf(" and ", System.StringComparison.Ordinal) >= 0
               || expr.StartsWith("not ", System.StringComparison.Ordinal);
    }

    bool EvaluateBoolExpr(string expr)
    {
        expr = expr.Trim();

        string[] parts = expr.Split(new[] { " and " }, System.StringSplitOptions.None);
        bool result = true;

        foreach (string part in parts)
        {
            result = result && EvaluateBoolAtom(part.Trim());
        }

        return result;
    }

    bool EvaluateBoolAtom(string expr)
    {
        expr = expr.Trim();

        if (expr.StartsWith("not ", System.StringComparison.Ordinal))
            return !EvaluateBoolAtom(expr.Substring(4).Trim());

        if (expr == "True") return true;
        if (expr == "False") return false;

        if (variables.ContainsKey(expr))
        {
            object v = variables[expr];
            if (v is bool b)
                return b;
            if (bool.TryParse(v.ToString(), out bool parsed))
                return parsed;
        }

        Debug.LogWarning($"Булево выражение: не удалось вычислить \"{expr}\".");
        return false;
    }

    float ResolveNumberOrVariable(string token)
    {
        token = token.Trim();

        if (TryParseNumber(token, out float num))
            return num;

        if (variables.ContainsKey(token))
        {
            object val = variables[token];
            if (val is float f) return f;
            if (val is int i) return i;
            if (val is string s && TryParseNumber(s, out float parsedFromString)) return parsedFromString;
        }

        Debug.LogWarning($"Не удалось интерпретировать \"{token}\" как число, подставлен 0.");
        return 0f;
    }

    bool TryParseNumber(string text, out float number)
    {
        string normalized = text.Trim().Replace(',', '.');
        return float.TryParse(normalized, NumberStyles.Float, CultureInfo.InvariantCulture, out number);
    }

    int ParseLoopCount(string arg)
    {
        if (string.IsNullOrWhiteSpace(arg))
            return -1;
        arg = arg.Trim();
        if (TryParseNumber(arg, out float n))
            return Mathf.Max(0, (int)Mathf.Floor(n));
        if (variables.ContainsKey(arg))
            return Mathf.Max(0, (int)Mathf.Floor(ResolveNumberOrVariable(arg)));
        return -1;
    }

    static string StripLeadingIndentUnits(string line, int unitsToRemove)
    {
        int consumed = 0;
        int idx = 0;
        while (idx < line.Length && consumed < unitsToRemove)
        {
            char c = line[idx];
            if (c == ' ')
            {
                consumed++;
                idx++;
            }
            else if (c == '\t')
            {
                consumed += 4;
                idx++;
            }
            else
                break;
        }

        return idx < line.Length ? line.Substring(idx) : "";
    }

    static string BuildDedentBlock(List<string> rawLines, int startIdx, int endIdxInclusive)
    {
        if (startIdx > endIdxInclusive)
            return "";

        int blockIndent = -1;
        for (int k = startIdx; k <= endIdxInclusive; k++)
        {
            string t = StripCommentStatic(rawLines[k]).TrimEnd();
            if (string.IsNullOrWhiteSpace(t))
                continue;
            blockIndent = GetIndent(rawLines[k]);
            break;
        }

        if (blockIndent < 0)
            return "";

        var sb = new StringBuilder();
        for (int k = startIdx; k <= endIdxInclusive; k++)
        {
            string raw = rawLines[k];
            string t = StripCommentStatic(raw).TrimEnd();
            if (string.IsNullOrWhiteSpace(t))
            {
                sb.AppendLine();
                continue;
            }

            if (GetIndent(raw) < blockIndent)
                return null;

            sb.AppendLine(StripLeadingIndentUnits(raw, blockIndent));
        }

        return sb.ToString().TrimEnd();
    }

    string StripComment(string line)
    {
        return StripCommentStatic(line);
    }
}
