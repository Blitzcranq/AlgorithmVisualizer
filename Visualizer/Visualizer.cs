/*
 * Copyright (C) 2024 Trent University. All Rights Reserved.
 *
 * Author(s):
 *  - Matthew Brown <matthewbrown@trentu.ca>
 */

namespace LabViz;

using Rendering;


public class SortVisualizer
{
    private const int MAX_VALUE = 1000; // Highest value for random number generation

    /// <summary>
    /// The items that actually get sorted by the user.
    /// </summary>
    public readonly int[] Items;

    // -----------------------------------------------------------------------------------------------------------------

    #region Sorting functions

    /// <summary>
    /// Calls <c cref="int.CompareTo(int)">CompareTo</c> on the two values <b>at the indices</b> <c>Items[indexL]</c>
    /// and <c>Items[indexR]</c>, returning its result.
    /// </summary>
    /// <param name="indexL">The <b>index</b> of the first item to compare.</param>
    /// <param name="indexR">The <b>index</b> of the second item to compare.</param>
    /// <returns>
    ///   <list type="bullet">
    ///     <item>
    ///       <term>Less than zero</term>
    ///       <description>The item at the left index is less than the one at the right index.</description>
    ///     </item>
    ///     <item>
    ///       <term>Zero</term>
    ///       <description>The item at the left index is equal to the one at the right index.</description>
    ///     </item>
    ///     <item>
    ///       <term>Greater than zero</term>
    ///       <description>The item at the left index is greater than the one at the right index.</description>
    ///     </item>
    ///   </list>
    /// </returns>
    public int Compare(int indexL, int indexR)
    {
        int compare = Items[indexL].CompareTo(Items[indexR]);

        Frames.Add(new CompareFrame(Frames[^1], indexL, indexR));

        return compare;
    }

    /// <summary>
    /// Swaps the values <b>at the indices</b> <c><paramref name="indexL"/></c> and <c><paramref name="indexR"/></c>.
    /// </summary>
    /// <param name="indexL">The <b>index</b> of the item to swap with the one at <paramref name="indexR"/>.</param>
    /// <param name="indexR">The <b>index</b> of the item to swap with the one at <paramref name="indexL"/>.</param>
    public void Swap(int indexL, int indexR)
    {
        ref int l = ref Items[indexL];
        ref int r = ref Items[indexR];
        (l, r) = (r, l);

        Frames.Add(new SwapFrame(Frames[^1], indexL, indexR));
    }

    /// <summary>
    /// Starts highlighting all of the boxes from <paramref name="startIndex"/> up to and including <paramref
    /// name="endIndex"/>.
    /// </summary>
    /// <param name="startIndex">The index of the first box to highlight.</param>
    /// <param name="endIndex">The (inclusive) index of the last box to highlight.</param>
    /// <param name="name">The name of this range (why it's being highlighted).</param>
    public void MarkRange(int startIndex, int endIndex, string name)
    {
        Frames.Add(new MarkRangeFrame(Frames[^1], (startIndex, endIndex), name));
    }

    /// <summary>
    /// Un-highlights whatever range is currently highlighted, if there is one.
    /// </summary>
    public void UnmarkRange()
    {
        var prev = Frames[^1];
        if (prev.MarkedRange is not null)
            Frames.Add(new UnmarkRangeFrame(prev));
    }

    /// <summary>
    /// Marks a specific <b>value</b> as of-interest (e.g., a minimum value or a pivot value). All boxes with this value
    /// will be drawn in-colour.
    /// </summary>
    /// <param name="value">Which value to start highlighting.</param>
    /// <param name="name">What this value is being marked as (e.g. "minimum").</param>
    public void MarkValue(int value, string name)
    {
        Frames.Add(new MarkValueFrame(Frames[^1], value, name));
    }

    /// <summary>
    /// Stops marking boxes with the current "marked value."
    /// </summary>
    public void UnmarkValue()
    {
        var prev = Frames[^1];
        if (prev.MarkedValue is not null)
            Frames.Add(new UnmarkValueFrame(prev));
    }

    #endregion

    // --------------------------------------------------------------------------------------------
    // --------------------------------------------------------------------------------------------
    // --------------------------------------------------------------------------------------------

    #region

    /// <summary>
    /// The name of the sorting algorithm method that this visualizer was called with.
    /// </summary>
    protected string? ActionName;

    /// <summary>
    /// An in-order list of the frames of the animation as they are generated.
    /// </summary>
    protected List<Frame> Frames;

    /// <summary>
    /// The internal MonoGame <c cref="Microsoft.Xna.Framework.Game">Game</c> that performs the visualization of the
    /// sorting algorithm.
    /// </summary>
    protected Renderer? Renderer;


    /// <summary>
    /// Spawns a new visualizer for the given algorithm.
    /// </summary>
    /// <param name="algorithm">The sorting algorithm function to call.</param>
    /// <param name="numItems">How many items fill the <c cref="Items">Items</c> array with.</param>
    public SortVisualizer(Action<SortVisualizer> algorithm, int numItems = 16)
    {
        // Need two copies of items, since we want to start the visualizer later with a fresh copy
        Items = new int[numItems];

        Random random = new();
        for (int i = 0; i < numItems; i++)
            Items[i] = random.Next(0, MAX_VALUE);

        Frames = new List<Frame>() { new() }; // Start with a single blank frame

        try
        {
            // Anonymous/lambda methods are sometimes things like `<Main>__foo1`. We try to only catch actual method
            // names.
            if (!algorithm.Method.IsSpecialName && !algorithm.Method.Name.Contains('<'))
                ActionName = algorithm.Method.Name;
            else
                ActionName = null;
        }
        catch (MemberAccessException)
        {
            ActionName = null;
        }

        // Run the algorithm
        algorithm(this);
    }


    /// <summary>
    /// Renders a visualization of the algorithm that this visualizer ran.
    /// </summary>
    /// <param name="msPerFrame">How many milliseconds the visualizer should wait before drawing each frame.</param>
    public void RenderVisualization(int msPerFrame = 100, int windowWidth = 800, int windowHeight = 640)
    {
        // Check if the sort was correct
        bool sortValid = CheckSorted(Items);
        if (!sortValid)
            Console.Error.WriteLine("Your sorting algorithm doesn't seem to have sorted the array correctly.");

        // Push the final frame onto the stack
        Frames.Add(new FinalFrame(Frames[^1], sortValid));

        // Then create the renderer and start the MonoGame window.
        using var renderer = new Renderer(Items, Frames, windowWidth, windowHeight)
        {
            FrameDelay = new TimeSpan(msPerFrame * TimeSpan.TicksPerMillisecond),
        };

        if (ActionName is string title)
            renderer.WindowTitle = title;

        renderer.Run();
    }


    /// <summary>
    /// Initialize and render a sorting algorithm visualization for the given algorithm.
    /// </summary>
    /// <param name="algorithm">The sorting algorithm function to call.</param>
    /// <param name="numItems">How many items fill the <c cref="Items">Items</c> array with.</param>
    /// <param name="msPerFrame"></param>
    public static void Visualize(Action<SortVisualizer> algorithm, int numItems = 16, int msPerFrame = 100)
    {
        // Initialize and render in one fell swoop, instead of allowing time to inspect in between. The only reason this
        // is done in two separate classes is just to make this class that the students interact with less insane. The
        // top half of this file should be fairly user-friendly to read. The class can't be totally static (without some
        // reorganization) because they need to have a `visualizer` object to interact with in order to perform
        // comparisons/swaps etc.
        new SortVisualizer(algorithm, numItems)
            .RenderVisualization(msPerFrame);
    }


    /// <summary>
    /// Double checks that an array is sorted.
    /// </summary>
    public static bool CheckSorted<T>(T[] array) where T : IComparable<T>
    {
        // Borrowed from: https://stackoverflow.com/a/57278839/10549827

        int end = array.Length - 1;
        if (end < 1) return true;

        // Loop forward as long as the current element belongs before the next.
        int i = 0;
        while (i < end && array[i].CompareTo(array[i + 1]) <= 0)
            i += 1;

        // If we made it all the way to the end, the array is sorted.
        return i == end;
    }

    #endregion
}
