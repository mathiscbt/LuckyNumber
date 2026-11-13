using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SelectionHighlighter
{
    private const float SelectedAlpha = 1f;
    private const float UnselectedAlpha = 0.6f;

    private readonly Dictionary<string, List<Button>> groups = new Dictionary<string, List<Button>>();
    private readonly Dictionary<Button, string> groupOf = new Dictionary<Button, string>();
    private readonly Dictionary<Button, string> methodOf = new Dictionary<Button, string>();

    public void Wire(Transform root)
    {
        foreach (Button button in root.GetComponentsInChildren<Button>(true))
        {
            if (button.onClick.GetPersistentEventCount() == 0) continue;

            string method = button.onClick.GetPersistentMethodName(0);
            string group = GroupName(method);
            if (group == null) continue;

            SumoSetupPanel owner = button.GetComponentInParent<SumoSetupPanel>();
            string key = group + (owner != null ? owner.GetInstanceID() : 0);

            if (!groups.ContainsKey(key)) groups[key] = new List<Button>();
            groups[key].Add(button);
            groupOf[button] = key;
            methodOf[button] = method;

            button.onClick.AddListener(() => Highlight(button));
        }

        foreach (Button button in groupOf.Keys)
        {
            SetOpacity(button, UnselectedAlpha);
        }
    }

    public void Select(string method)
    {
        foreach (var pair in methodOf)
        {
            if (pair.Value == method)
            {
                Highlight(pair.Key);
                return;
            }
        }
    }

    private void Highlight(Button clicked)
    {
        foreach (Button button in groups[groupOf[clicked]])
        {
            SetOpacity(button, button == clicked ? SelectedAlpha : UnselectedAlpha);
        }
    }

    private static void SetOpacity(Button button, float alpha)
    {
        foreach (Graphic graphic in button.GetComponentsInChildren<Graphic>(true))
        {
            Color color = graphic.color;
            color.a = alpha;
            graphic.color = color;
        }
    }

    private static string GroupName(string method)
    {
        switch (method)
        {
            case "SelectLuckyNumber":
            case "RandomizeLuckyNumber":
                return "lucky";
            case "SelectColor":
                return "color";
            case "OnClickNormal":
            case "OnClickHard":
                return "difficulty";
            default:
                return null;
        }
    }
}
