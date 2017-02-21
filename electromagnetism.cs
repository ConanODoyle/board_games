package electromagnetism
{
	function Brickgroup_39943::onAdd(%this, %brick)
	{
		%p = parent::onAdd(%this, %brick);

		talk(%brick.getColorId());

		return %p;
	}
}