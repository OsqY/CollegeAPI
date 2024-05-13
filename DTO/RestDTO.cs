namespace CollegeAPI.DTO
{
    public class RestDTO<T>
    {
        public T Data { get; set; }

        public List<LinkDTO> Links { get; set; } = new List<LinkDTO>();
    }
}
