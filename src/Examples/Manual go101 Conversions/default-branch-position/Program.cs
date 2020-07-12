// https://go101.org/article/details.html#default-branch-position
/*
	switch n := rand.Intn(3); n {
	case 0: fmt.Println("n == 0")
	case 1: fmt.Println("n == 1")
	default: fmt.Println("n == 2")
	}

	switch n := rand.Intn(3); n {
	default: fmt.Println("n == 2")
	case 0: fmt.Println("n == 0")
	case 1: fmt.Println("n == 1")
	}

	switch n := rand.Intn(3); n {
	case 0: fmt.Println("n == 0")
	default: fmt.Println("n == 2")
	case 1: fmt.Println("n == 1")
	}

	var x, y chan int

	select {
	case <-x:
	case y <- 1:
	default:
	}

	select {
	case <-x:
	default:
	case y <- 1:
	}

	select {
	default:
	case <-x:
	case y <- 1:
	}
*/
#region source
using go;
using fmt = go.fmt_package;
using rand = go.math.rand_package;

static class main_package
{
    static void Main() {
        {
			int n = rand.Intn(3);
            switch (n) {
				case 0: fmt.Println("n == 0"); break;
				case 1: fmt.Println("n == 1"); break;
				default: fmt.Println("n == 2"); break;
            }
        }

        {
            int n = rand.Intn(3);
            switch (n)
            {
                default: fmt.Println("n == 2"); break;
                case 0: fmt.Println("n == 0"); break;
                case 1: fmt.Println("n == 1"); break;
            }
        }
        
        {
			int n = rand.Intn(3);
            switch (n)
            {
                case 0: fmt.Println("n == 0"); break;
                default: fmt.Println("n == 2"); break;
                case 1: fmt.Println("n == 1"); break;
            }
        }

		channel<int> x = default, y = default;

		switch (true)
        {
			case true when x.Received(out _): break;
            case true when y.Sent(1): break;
			default: break;
        }

        switch (true)
        {
            case true when x.Received(out _): break;
            default: break;
            case true when y.Sent(1): break;
        }

        switch (true)
        {
            default: break;
            case true when x.Received(out _): break;
            case true when y.Sent(1): break;
        }
	}
}
#endregion
namespace go.math
{
    static class rand_package
    {
        private static readonly System.Random s_random = new System.Random();

        public static int Intn(int range) =>
            s_random.Next(0, range);
    }
}

// Important conversion takeaway:
