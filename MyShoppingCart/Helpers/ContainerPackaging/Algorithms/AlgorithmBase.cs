using MyshoppingCart.Helpers.ContainerPackaging.Entities;
using System.Collections.Generic;

namespace MyshoppingCart.Helpers.ContainerPackaging.Algorithms
{
	public abstract class AlgorithmBase
	{
		public abstract ContainerPackingResult Run(Container container, List<Item> items);
	}
}