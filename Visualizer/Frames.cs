/*
 * Copyright (C) 2024 Trent University. All Rights Reserved.
 *
 * Author(s):
 *  - Matthew Brown <matthewbrown@trentu.ca>
 */

using Microsoft.Xna.Framework;

namespace LabViz.Rendering;


public record Frame
{
    // Properties of a frame that persist across frames; subtypes include single-frame specific metadata.
    public (int, string)? MarkedValue;
    public ((int, int), string)? MarkedRange;

    public Frame()
    {
        MarkedValue = null;
        MarkedRange = null;
    }

    public Frame(Frame previous)
    {
        MarkedValue = previous.MarkedValue;
        MarkedRange = previous.MarkedRange;
    }

    public virtual string? Describe(int[] itemValues)
    {
         // By default, this frame signals no changes to the scene
        return null;
    }

    public virtual Color GetBoxColor(int itemIdx, int itemVal)
    {
        if (MarkedRange is ((int start, int end), _) && start <= itemIdx && itemIdx <= end)
            return new Color(255, 255, 100);
        // else if (MarkedValue is (int value, _) && value == itemVal)
        //     return Color.Blue;
        else
            return Color.White;
    }

    // Methods that need to mutate the actual item state
    public virtual void ApplyMutation(int[] itemValues) { return; }
    public virtual void UndoMutation(int[] itemValues) { return; }
}

// =====================================================================================================================

public record SwapFrame : Frame
{
    public readonly int L;
    public readonly int R;

    public SwapFrame(Frame previous, int indexL, int indexR) : base(previous)
    {
        L = indexL;
        R = indexR;
    }

    public override string? Describe(int[] itemValues)
    {
        return $"Swap elements #{L} and #{R}.";
    }

    public override Color GetBoxColor(int itemIdx, int itemVal)
    {
        if (itemIdx == L || itemIdx == R) return Color.LimeGreen;
        else return base.GetBoxColor(itemIdx, itemVal);
    }

    public override void ApplyMutation(int[] itemValues)
    {
        ref int l = ref itemValues[L];
        ref int r = ref itemValues[R];
        (l, r) = (r, l);
    }

    // A swap is its own inverse
    public override void UndoMutation(int[] itemValues) => ApplyMutation(itemValues);
}

// =====================================================================================================================

public record CompareFrame : Frame
{
    public int L;
    public int R;

    public CompareFrame(Frame previous, int indexL, int indexR) : base(previous)
    {
        L = indexL;
        R = indexR;
    }

    public override string? Describe(int[] itemValues)
    {
        int lVal = itemValues[L];
        int rVal = itemValues[R];
        return $"Compare element #{L} ({lVal}) with element #{R} ({rVal}).";
    }

    public override Color GetBoxColor(int itemIdx, int itemVal)
    {
        if (itemIdx == L || itemIdx == R) return Color.Red;
        else return base.GetBoxColor(itemIdx, itemVal);
    }
}

// =====================================================================================================================

public record MarkValueFrame : Frame
{
    public MarkValueFrame(Frame previous, int value, string name) : base(previous)
    {
        MarkedValue = (value, name);
    }

    public override string? Describe(int[] itemValues)
    {
        var (value, name) = MarkedValue!.Value; // if this is a mark frame, we know this property is set
        return $"Mark the value {value} as {name}.";
    }
}

// =====================================================================================================================

public record MarkRangeFrame : Frame
{
    public MarkRangeFrame(Frame previous, (int, int) range, string name) : base(previous)
    {
        MarkedRange = (range, name);
    }

    public override string? Describe(int[] itemValues)
    {
        var ((l, r), name) = MarkedRange!.Value;
        if (l == r) return $"Mark element #{l} as {name}.";
        else return $"Mark the elements #{l} to #{r} as {name}.";
    }
}

// =====================================================================================================================

public record UnmarkValueFrame : Frame
{
    public string Name;

    public UnmarkValueFrame(Frame previous) : base(previous)
    {
        if (MarkedValue is not (_, string name))
            throw new InvalidOperationException("There is no marked value to unset.");

        Name = name;
        MarkedValue = null;
    }

    public override string? Describe(int[] itemValues) => $"Un-mark the value '{Name}'.";
}

// =====================================================================================================================

public record UnmarkRangeFrame : Frame
{
    public string Name;

    public UnmarkRangeFrame(Frame previous) : base(previous)
    {
        if (MarkedRange is not (_, string name))
            throw new InvalidOperationException("There is no marked range to unset.");

        Name = name;
        MarkedRange = null;
    }

    public override string? Describe(int[] itemValues) => $"Un-mark the range '{Name}'";
}

// =====================================================================================================================

public record FinalFrame : Frame
{
    public bool SortCorrect;

    public FinalFrame(Frame previous, bool correct) : base(previous)
    {
        SortCorrect = correct;
        MarkedRange = null;
        MarkedValue = null;
    }

    public override Color GetBoxColor(int itemIdx, int itemVal) => SortCorrect ? Color.LimeGreen : Color.Red;

    public override string? Describe(int[] itemValues) => SortCorrect
        ? "Done! Values sorted."
        : "Done! Almost... your algorithm isn't quite right!";
}
