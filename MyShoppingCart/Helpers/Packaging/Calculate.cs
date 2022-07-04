using MyShoppingCart.Models;
using System.Collections.Generic;

namespace MyShoppingCart.Helpers.Packaging
{
    public class Calculate
    {
        /// <summary>
        /// Given dimensions in inches, calculate girth
        /// </summary>
        /// <param name="length" unit="whole inches"></param>
        /// <param name="width" unit="whole inches"></param>
        /// <param name="height" unit="whole inches"></param>
        /// <returns></returns>
        public static int CalculateGirth(int length, int width, int height)
        {
            /*
             * Add the two smallest dimensions and multiply by two. This is your package’s girth.
             * https://support.ordoro.com/how-do-i-calculate-the-girth-and-length-of-a-package-using-the-parcel-dimensions/
             */

            int iSmall1 = 0;
            int iSmall2 = 0;

            if (length >= width)
                iSmall1 = width;

            if (length >= height)
                if (iSmall1 == 0)
                    iSmall1 = height;
                else
                {
                    iSmall2 = height;
                    return (iSmall1 + iSmall2) * 2;
                }

            if (width >= length)
                if (iSmall1 == 0)
                    iSmall1 = length;
                else
                {
                    iSmall2 = length;
                    return (iSmall1 + iSmall2) * 2;
                }

            if (width >= height)
                if (iSmall1 == 0)
                    iSmall1 = height;
                else
                {
                    iSmall2 = height;
                    return (iSmall1 + iSmall2) * 2;
                }

            if(height >= length)
                if(iSmall1 == 0)
                    iSmall1= length;
                else
                {
                    iSmall2 = length;
                    return (iSmall1 + iSmall2) * 2;
                }

            if(height >= width)
                if(iSmall1 == 0)
                    iSmall1= width;
                else
                {
                    iSmall2 = width;
                    return (iSmall1 + iSmall2) * 2;
                }

            return 0;
        }

        public static int GetLongestSide(int length, int width, int height)
        {
            int iLongestSide = 0;

            if(length >= width && length >= height)            
                iLongestSide = length;

            if (width >= length && width >= height)
                iLongestSide = width;

            if (height >= length && height >= width)
                iLongestSide = height;

            return iLongestSide;
        }

        public static int GetCombinedLengthAndGirth(int length, int width, int height)
        {
            return CalculateGirth(length, width, height) + GetLongestSide(length, width, height);
        }

        public static decimal CalculateShipping(List<ShoppingCartItem> shoppingCartItems)
        {
            decimal shipping = 0;

            foreach(ShoppingCartItem shoppingCartItem in shoppingCartItems)
            {

            }

            return shipping;
        }
    }
}
