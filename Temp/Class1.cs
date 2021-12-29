using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpLogix.Core
{
	class Class1
	{
		int TestIntA;
		int TestIntB = 3;
		int TestIntC;

		void Start()
		{
			TestIntA = 2;

			TestIntC = TestIntA + TestIntB;
		}
	}
}
