using LabViz;


class Program
{
    #region Main program


    /// <summary>
    /// Determines the number of boxes to test sorting.
    /// </summary>
    public const int numItems = 15;

    /// <summary>
    /// Determines how quickly the animation should run (60 fps = 16.67 ms/frame). Set to 0 to run as fast as possible.
    /// </summary>
    public const int msPerFrame = 50;


    static void Main()
    {
        /* Uncomment each algorithm's respective line to see it visualized. */

        // SortVisualizer.Visualize(SelectionSort, numItems, msPerFrame);
        // SortVisualizer.Visualize(InsertionSort, numItems, msPerFrame);
        // SortVisualizer.Visualize(BubbleSort, numItems, msPerFrame);
        SortVisualizer.Visualize(ShellSort, numItems, msPerFrame);
    }


    #endregion


    #region Sorting algorithms


    /* Selection sort has been done for you to serve as an example for how to use the visualizer. */
    static void SelectionSort(SortVisualizer visualizer)
    {
        var items = visualizer.Items;
        var n = items.Length;

        for (int i = 0; i < n - 1; i++)
        {
            // Find the minimum value from the unsorted list; the first `i` elements have already been sorted into
            // place, so we can start at `i` (true even first time through, since the first 0 elements have been
            // sorted).
            int minimum = i;
            visualizer.MarkValue(items[i], "minimum");

            for (int j = i + 1; j < n; j++)
            {
                // If this is a new minimum, keep track of it instead.
                if (visualizer.Compare(j, minimum) < 0)
                {
                    minimum = j;
                    visualizer.MarkValue(items[j], "minimum");
                }
            }

            visualizer.Swap(minimum, i);            // Swap the minimum to the start of the unsorted part
            visualizer.MarkRange(0, i, "sorted");   // Everything up to `i` has now been sorted, continue.
        }
    }

    // =================================================================================================================

    static void InsertionSort(SortVisualizer visualizer)
    {
        var items = visualizer.Items;
        var n = items.Length;

        for (int i = 0; i < n - 1; i++)
        {

            for (int j = i + 1; j > 0; j--)
            {
                visualizer.MarkValue(items[i], "minimum");
                // Swap if the element at j - 1 position is greater than the element at j position
                if (visualizer.Compare(j - 1, j) > 0)
                {
                    visualizer.Swap(j - 1, j);

                }
            }
        }
    }

    // =================================================================================================================

    static void BubbleSort(SortVisualizer visualizer)
    {
        var items = visualizer.Items;
        var n = items.Length;

        for (int i = 0; i < n - 1; i++)
        {
            for (int j = 0; j < n - i - 1; j++)
            {
                if (visualizer.Compare(j, j + 1) > 0)
                {
                    visualizer.Swap(j, j + 1);
                }
            }
        }

        // Replace this line with your implementation!
    }

    // =================================================================================================================

    static void ShellSort(SortVisualizer visualizer)
    {
        var items = visualizer.Items;
        var n = items.Length;

        for (int gap = n / 2; gap > 0; gap /= 2)
        {
            // Do a gapped insertion sort for this gap size.
            // The first gap elements a[0..gap-1] are already
            // in gapped order keep adding one more element
            // until the entire array is gap sorted
            for (int i = gap; i < n; i += 1)
            {
                // add a[i] to the elements that have
                // been gap sorted save a[i] in temp and
                // make a hole at position i
                int temp = items[i];

                // shift earlier gap-sorted elements up until
                // the correct location for a[i] is found
                int j;
                for (j = i; j >= gap && visualizer.Compare(j - gap, j) > 0; j -= gap)
                    visualizer.Swap(j - gap, j);

                // put temp (the original a[i]) 
                // in its correct location
                items[j] = temp;
            }
        }
    }

    // =================================================================================================================

    /* Add your own extra algorithms down here! 👀 */

    #endregion
    //Question-2:
    //Write a short sentence or two describing each one in terms of how the boxes change colours and move around the screen. 
    //Insertion Sort:
    /**
    In insertion sort, the algorithm will compare the current(first in the first case) with all the previous elements. 
    If the current element is smaller than the previous element, the algorithm swaps the two elements. 
    This process continues until the current element is greater than all the previous elements. The algorithm will then move to the next element and repeat the process for all elements.
    **/
    //Bubble Sort:
    /**
    In bubble sort, the algorithm will compare the current element with the next element. 
    If the current element is greater than the next element, the algorithm will swap the two elements.
    This process continues until the algorithm reaches the end of the list.
    The algorithm will then repeat the process for all elements until the list is sorted.
    **/

    //Question-3:
    //3.	Based on what you saw in the visualizations, and a bit of reasoning, which of the algorithms seems like it'd be most efficient? Why might that be? Write another quick sentence or two explaining why.
    /**


    Based on the visualizations, the Shell Sort algorithm seems like it would be the most efficient.
    This is because Shell Sort uses a gap value to sort elements that are far apart, which allows it to quickly reduce the number of inversions in the list.
    As the algorithm progresses, the gap value decreases, allowing the algorithm to sort elements that are closer together.
    This process helps to reduce the number of comparisons and swaps needed to sort the list, making Shell Sort more efficient than the other algorithms.

    **/

}
