Converts specified white and black image to ASCII-art.
<pre>
Usage: AsciiArtGenerator &lt;input_image&gt; [/P] [/B &lt;beta&gt;] [/T &lt;threshold&gt;] 
       [/I &lt;iterations_count&gt;] [/N &lt;threads_number&gt;] [/O &lt;output_file&gt;]

Options:
   /P                       Use this option to convert the image using the
                            pseudoinverse.
   /B &lt;beta&gt;                Sets 'beta' parameter, that affects cost function
                            (ignored if /P is set). Default value is 2.0.
   /T &lt;threshold&gt;           Sets threshold for maximum activation values.
                            Possible values are from 0.0 to 1.0.
                            Default value is 0.0.
   /I &lt;iterations_count&gt;    Sets number of iterations of an algorithm.
                            Possible values are from 1 to 65535
                            (ignored if /P is set). Default value is 100.
   /N &lt;threads_number&gt;      Sets maximum threads number.
                            Possible values are from 1 to 65535
                            (ignored if /P is set). Default value is 1.
   /O &lt;output_file&gt;         Sets name of output HTML file.
   /?                       Show help
</pre>
