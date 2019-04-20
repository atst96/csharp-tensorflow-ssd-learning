using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TensorFlow;

namespace TensorFlowSharpSSD
{
    internal static class Extensions
    {
        public static T GetValue<T>(this TFTensor tensor, bool jagged = true)
        {
            return (T)tensor.GetValue(jagged);
        }

        public static T GetValue<T>(this TFTensor[] tensor, int index, bool jagged = true)
        {
            return tensor[index].GetValue<T>(jagged);
        }
    }
}
