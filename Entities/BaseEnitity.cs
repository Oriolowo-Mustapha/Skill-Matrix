using MassTransit;
using System.ComponentModel.DataAnnotations;

namespace Skill_Matrix.Entities
{
	public class BaseEnitity
	{
		[Key]
		public Guid Id { get; set; } = NewId.Next().ToGuid();
	}
}
