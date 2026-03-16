using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
using UnityEngine;

public class InterpreterEngine
{
    public Dictionary<string, object> variables = new Dictionary<string, object>();
    public string lastPrintedValue = "";

    public bool Execute(string code)
    {
        if (string.IsNullOrWhiteSpace(code))
            return false;

        bool anyExecuted = false;

        foreach (string statement in SplitStatements(code))
        {
            string line = StripComment(statement).Trim();
            if (string.IsNullOrEmpty(line))
                continue;

            if (!ExecuteSingle(line))
                return false;

            anyExecuted = true;
        }

        return anyExecuted;
    }

    bool ExecuteSingle(string codeLine)
    {
        // print()
        Match printMatch = Regex.Match(codeLine, @"^print\s*\((.*)\)$");
        if (printMatch.Success)
        {
            string content = Evaluate(printMatch.Groups[1].Value);
            lastPrintedValue = content;
            return true;
        }

        // assignment
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

        // ������
        Match strMatch = Regex.Match(expr, @"['""](.*?)['""]");
        if (strMatch.Success)
            return strMatch.Groups[1].Value;

        // ����������
        if (variables.ContainsKey(expr))
            return variables[expr].ToString();

        return expr;
    }

    object EvaluateRaw(string expr)
    {
        expr = expr.Trim();

        // ������� ���������: a * b (����� ��� ����������)
        int mulIndex = expr.IndexOf('*');
        if (mulIndex > 0)
        {
            string left = expr.Substring(0, mulIndex).Trim();
            string right = expr.Substring(mulIndex + 1).Trim();

            float leftVal = ResolveNumberOrVariable(left);
            float rightVal = ResolveNumberOrVariable(right);

            return leftVal * rightVal;
        }

        // ������
        Match strMatch = Regex.Match(expr, @"['""](.*?)['""]");
        if (strMatch.Success)
            return strMatch.Groups[1].Value;

        // ����� (0.85 ��� 0,85)
        if (TryParseNumber(expr, out float number))
            return number;

        // bool
        if (expr == "True") return true;
        if (expr == "False") return false;

        // ����������
        if (variables.ContainsKey(expr))
            return variables[expr];

        return expr;
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

        Debug.LogWarning($"�� ������� ���������������� \"{token}\" ��� �����, ��������� 0.");
        return 0f;
    }

    bool TryParseNumber(string text, out float number)
    {
        string normalized = text.Trim().Replace(',', '.');
        return float.TryParse(normalized, NumberStyles.Float, CultureInfo.InvariantCulture, out number);
    }

    System.Collections.Generic.IEnumerable<string> SplitStatements(string code)
    {
        // ��������� ���� �� "���������" ��:
        // - �������� ������
        // - ';'
        // - ',' (������ �� ������� ������, ��� ����� � ��� ������)
        // ��� ��������� ��������� ������ ������� ����� �������.
        var sb = new System.Text.StringBuilder();

        bool inSingle = false;
        bool inDouble = false;
        bool escape = false;
        int parenDepth = 0;

        for (int i = 0; i < code.Length; i++)
        {
            char c = code[i];

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

    string StripComment(string line)
    {
        bool inSingle = false;
        bool inDouble = false;
        bool escape = false;

        for (int i = 0; i < line.Length; i++)
        {
            char c = line[i];

            if (escape)
            {
                escape = false;
                continue;
            }

            if (c == '\\')
            {
                escape = true;
                continue;
            }

            if (!inDouble && c == '\'') inSingle = !inSingle;
            else if (!inSingle && c == '"') inDouble = !inDouble;

            if (!inSingle && !inDouble && c == '#')
                return line.Substring(0, i);
        }

        return line;
    }
}