


using CleanArchitecture.Infrastructure.Interface;
using VideoGameApi;

public class VideoGameRepository(ApplicationDbContext context) : GenericRepository<VideoGame>(context), IVideoGameRepository { }
