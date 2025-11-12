#import <AudioToolbox/AudioToolbox.h> // old (< iOS 6.0)
#import <AVFoundation/AVFoundation.h> // new (>= iOS 6.0)
#import <MediaPlayer/MediaPlayer.h>


typedef void (*IsOtherAudioPlayingChangedCallback)(int, void*);

//---------------------------------------------------------------------
// Interface to check any audio event from iOS system
// and inform user through a callback ('audioInterruptionCallback')
// or with a property ('isOtherAudioIsPlaying').
//---------------------------------------------------------------------
@interface OtherAudioChecker : NSObject

+ (OtherAudioChecker*)instance; // obtain singleton

@property (readonly, atomic) BOOL isOtherAudioIsPlaying; //
@property (atomic) IsOtherAudioPlayingChangedCallback audioInterruptionCallback;
@property (atomic) void* audioInterruptionCallbackData;

@end

@implementation OtherAudioChecker
{
    BOOL _isOnInterruption;
    BOOL _isSystemMusicPlaying;
}

// singleton
+ (OtherAudioChecker*)instance {
    static OtherAudioChecker* instance = nil;
    @synchronized(self) {
        if (instance == nil) {
            instance = [[self alloc] init];
        }
    }
    return instance;
}

- (id)init {
    if ((self = [super init]))
    {
        _isOnInterruption = false;
        _isSystemMusicPlaying = false;
        _audioInterruptionCallback = NULL;
        _audioInterruptionCallbackData = NULL;
        _isOtherAudioIsPlaying = NO;
        // register audio session interruption notification
        NSNotificationCenter* center = [NSNotificationCenter defaultCenter];
        [center addObserver:self selector:@selector(onAudioSessionInterruption:) name:AVAudioSessionInterruptionNotification object:nil];
        // register music player playback change status notification
        [center addObserver:self selector:@selector(onAudioSessionInterruption:) name:MPMusicPlayerControllerPlaybackStateDidChangeNotification object:nil];
        // register app did become active notification
        [center addObserver:self selector:@selector(onAppActive:) name:UIApplicationDidBecomeActiveNotification object:nil];
        
        // starts to generate notifications in systemMusic for our application
        MPMusicPlayerController* musicController = [MPMusicPlayerController respondsToSelector:@selector(systemMusicPlayer)] ? [MPMusicPlayerController systemMusicPlayer] : [MPMusicPlayerController iPodMusicPlayer];
        [musicController beginGeneratingPlaybackNotifications];
    }
    
    return self;
}

- (void)dealloc {
    // disable all notifications
    MPMusicPlayerController* musicController = [MPMusicPlayerController respondsToSelector:@selector(systemMusicPlayer)] ? [MPMusicPlayerController systemMusicPlayer] : [MPMusicPlayerController iPodMusicPlayer];
    [musicController endGeneratingPlaybackNotifications];
    
    [[NSNotificationCenter defaultCenter] removeObserver:self];
}

- (void)onAppActive:(NSNotification*)notification
{
    // workaround to unset interruptions that never sent "end" notification but application became active.
    if (_isOnInterruption == YES) {
        _isOnInterruption = NO;
        if (_isSystemMusicPlaying == NO) {
            _isOtherAudioIsPlaying = NO;
            if (_audioInterruptionCallback != NULL) {
                _audioInterruptionCallback(0, _audioInterruptionCallbackData);
            }
        }
    }
}

- (void)onAudioSessionInterruption:(NSNotification*)notification
{
    BOOL isPlaying = _isOtherAudioIsPlaying;
    
    MPMusicPlayerController* musicController = [MPMusicPlayerController respondsToSelector:@selector(systemMusicPlayer)] ? [MPMusicPlayerController systemMusicPlayer] : [MPMusicPlayerController iPodMusicPlayer];
    
    if ([musicController playbackState] == MPMusicPlaybackStatePlaying) {
        isPlaying = YES;
        _isSystemMusicPlaying = YES;
    } else {
        isPlaying = NO;
        _isSystemMusicPlaying = NO;
        
        if ([notification.name isEqualToString:AVAudioSessionInterruptionNotification]) {
            if ([[notification.userInfo valueForKey:AVAudioSessionInterruptionTypeKey] isEqualToNumber:[NSNumber numberWithInt:AVAudioSessionInterruptionTypeBegan]]) {
                isPlaying = YES;
                _isOnInterruption = YES;
            } else { // AVAudioSessionInterruptionTypeEnded
                // "There is no guarantee that a begin interruption will have an end interruption." - Apple
                isPlaying = NO;
                _isOnInterruption = NO;
            }
        }
    }
    
    if (isPlaying != _isOtherAudioIsPlaying) {
        _isOtherAudioIsPlaying = isPlaying;
        // fire callback
        if (_audioInterruptionCallback != NULL) {
            _audioInterruptionCallback(_isOtherAudioIsPlaying == YES ? 1 : 0, _audioInterruptionCallbackData);
        }
    }
}

@end

// Event based check.
// Callback fired if other audio starts or finishes to be played.
void register_other_audio_is_playing_function(IsOtherAudioPlayingChangedCallback callback, void* user_data)
{
    OtherAudioChecker* checker = [OtherAudioChecker instance];
    checker.audioInterruptionCallback = callback;
    checker.audioInterruptionCallbackData = user_data;
}

// Direct check.
int other_audio_is_playing(void)
{
    if ([[[UIDevice currentDevice] systemVersion] compare:@"6.0" options:NSNumericSearch] == NSOrderedAscending)
    {
        /* media_services_lost_handler のインスタンスを生成 */
        UInt32      value      = 1;
        UInt32      value_size = sizeof(value);
        OSStatus    result     = AudioSessionGetProperty(kAudioSessionProperty_OtherAudioIsPlaying, &value_size, &value);
        /* 他のアプリが音を出している場合は value == 1 */
        return (result == noErr) ? value : -1;
    } else {
        // check actual status of systemMusic (formerly iPodMusic)
        
        MPMusicPlayerController* musicController = [MPMusicPlayerController respondsToSelector:@selector(systemMusicPlayer)] ? [MPMusicPlayerController systemMusicPlayer] : [MPMusicPlayerController iPodMusicPlayer];
        if ([musicController playbackState] == MPMusicPlaybackStatePlaying) {
            return 1;
        }
        // check actual status of the iOS audio event checker (some notifications may have been received in a while)
        if ([[OtherAudioChecker instance] isOtherAudioIsPlaying] == YES) {
            return 1;
        }
        return 0;
    }
}
