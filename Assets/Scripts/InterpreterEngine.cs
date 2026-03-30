using System.Collections.Generic;
using System.Globalization;
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
            copy.variables[kv.Key] = kv.Value;
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
                if (c == ')' && parenDepth > 0) parenDepth--;

                bool isSeparator =
                    c == '\n' ||
                    c == '\r' ||
                    c == ';' ||
                    (c == ',' && parenDepth == 0);

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
            string content = Evaluate(printMatch.Groups[1].Value);
            lastPrintedValue = content;
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

    string Evaluate(string expr)
    {
        expr = expr.Trim();

        Match strMatch = Regex.Match(expr, @"['""](.*?)['""]");
        if (strMatch.Success)
            return strMatch.Groups[1].Value;

        if (variables.ContainsKey(expr))
            return variables[expr].ToString();

        return expr;
    }

    object EvaluateRaw(string expr)
    {
        expr = expr.Trim();

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

    string StripComment(string line)
    {
        return StripCommentStatic(line);
    }
}
