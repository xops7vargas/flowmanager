import React, { useState, useEffect, useRef } from 'react';
import {
  Box, Fab, Badge, Drawer, List, ListItem, ListItemAvatar, ListItemText,
  Avatar, Typography, TextField, IconButton, Paper, alpha, useTheme, 
  Button, InputAdornment, Snackbar, Alert, Menu, MenuItem
} from '@mui/material';
import { Chat, Send, Close, Person, Search, Add, ArrowBack, EmojiEmotions } from '@mui/icons-material';
import { useTranslation } from 'react-i18next';
import { useGetConversationMessagesQuery, 
  useSendMessageMutation, useMarkConversationReadMutation,
  useGetUsersQuery, useCreateConversationMutation } from '../../api';
import { Message, Conversation } from '../../types';
import { useAppSelector } from '../../hooks/useRedux';

type ChatWidgetProps = {
  conversations: Conversation[];
  onRefetch: () => void;
};

export default function ChatWidget({ conversations: propConversations, onRefetch }: ChatWidgetProps) {
  const { t } = useTranslation();
  const theme = useTheme();
  const [open, setOpen] = useState(false);
  const [showNewChat, setShowNewChat] = useState(false);
  const [selectedConversation, setSelectedConversation] = useState<Conversation | null>(null);
  const [message, setMessage] = useState('');
  const [searchUser, setSearchUser] = useState('');
  const messagesEndRef = useRef<HTMLDivElement>(null);
  const [lastMessageId, setLastMessageId] = useState<string>('');
  const [notificationOpen, setNotificationOpen] = useState(false);
  const [notificationMsg, setNotificationMsg] = useState('');
  const [emojiAnchor, setEmojiAnchor] = useState<null | HTMLElement>(null);
  
  const emojis = ['😊', '👍', '❤️', '🎉', '🙏', '😂', '😢', '😮', '😎', '❤️‍🔥', '👏', '🔥', '✅', '❌', '⭐', '💯'];
  
  const conversations = propConversations;
  const refetchConversations = onRefetch;
  const { data: messages, refetch: refetchMessages } = useGetConversationMessagesQuery(
    { conversationId: selectedConversation?.id || '', pageSize: 50 },
    { skip: !selectedConversation }
  );
  const { data: users, refetch: refetchUsers } = useGetUsersQuery({ page: 1, pageSize: 100 });
  const [sendMessage] = useSendMessageMutation();
  const [markRead] = useMarkConversationReadMutation();
  const [createConversation] = useCreateConversationMutation();
  const currentUser = useAppSelector((state) => state.auth.user);

  const totalUnread = conversations?.reduce((sum, c) => sum + c.unreadCount, 0) || 0;

  useEffect(() => {
    if (messagesEndRef.current) {
      messagesEndRef.current.scrollIntoView({ behavior: 'smooth' });
    }
  }, [messages]);

  useEffect(() => {
    if (!selectedConversation) return;
    
    const interval = setInterval(() => {
      refetchMessages();
      refetchConversations();
    }, 3000);
    
    return () => clearInterval(interval);
  }, [selectedConversation, refetchMessages, refetchConversations]);

  useEffect(() => {
    if (messages && messages.length > 0) {
      const latestId = messages[0].id;
      if (latestId !== lastMessageId) {
        setLastMessageId(latestId);
        refetchConversations();
      }
    }
  }, [messages, lastMessageId]);

  useEffect(() => {
    if (selectedConversation && open) {
      markRead(selectedConversation.id);
      refetchMessages();
    }
  }, [selectedConversation, open]);

  const handleOpenChat = () => {
    setOpen(true);
    refetchConversations();
    refetchUsers();
  };

  const handleCloseChat = () => {
    setOpen(false);
    setSelectedConversation(null);
    setShowNewChat(false);
  };

  const handleSendMessage = async () => {
    if (!message.trim() || !selectedConversation) return;
    
    try {
      await sendMessage({
        conversationId: selectedConversation.id,
        content: message,
        type: 0
      }).unwrap();
      setMessage('');
      refetchMessages();
      refetchConversations();
      const msg = t('chat.messageSent') || 'Mensaje enviado';
      setNotificationMsg(msg);
      setNotificationOpen(true);
    } catch (err) {
      console.error('Error sending message:', err);
    }
  };

  const handleSelectConversation = (conv: Conversation) => {
    setSelectedConversation(conv);
    setShowNewChat(false);
  };

  const handleStartChat = async (userId: string, userName: string) => {
    try {
      const existingConv = conversations?.find(c => 
        c.type === 0 && c.participants.some(p => p.userId === userId)
      );
      
      if (existingConv) {
        setSelectedConversation(existingConv);
        setShowNewChat(false);
        return;
      }

      const result = await createConversation({
        participantIds: [userId],
        type: 0,
        name: ''
      }).unwrap();
      setShowNewChat(false);
      refetchConversations();
      setSelectedConversation(result);
    } catch (error) {
      console.error('Error creating conversation:', error);
    }
  };

  const getConversationName = (conv: Conversation) => {
    if (conv.type === 0) {
      const other = conv.participants.find(p => p.userId !== currentUser?.id);
      return other?.userName || t('chat.directMessage');
    }
    return conv.name || t('chat.group');
  };

  const allUsersExceptMe = [
    ...(conversations?.filter(c => c.type === 0).map(c => {
      const other = c.participants.find(p => p.userId !== currentUser?.id);
      return other ? { id: other.userId, name: other.userName } : null;
    }).filter(Boolean) || []),
    ...(users?.items
      .filter((u: any) => u.id !== currentUser?.id && 
        !conversations?.some(c => c.type === 0 && c.participants.some(p => p.userId === u.id)))
      .map((u: any) => ({ id: u.id, name: `${u.firstName} ${u.lastName}` })) || [])
  ];

  const uniqueUsers = Array.from(new Map(allUsersExceptMe.map(u => [u.id, u])).values());

  const filteredUsers = uniqueUsers.filter(u => 
    u.name.toLowerCase().includes(searchUser.toLowerCase())
  );

  return (
    <>
      {!open && (
        <Fab
          color="primary"
          onClick={handleOpenChat}
          sx={{ position: 'fixed', bottom: 24, right: 24, zIndex: 9999 }}
        >
          <Badge badgeContent={totalUnread} color="error">
            <Chat />
          </Badge>
        </Fab>
      )}

      <Drawer
        anchor="right"
        open={open}
        onClose={handleCloseChat}
        PaperProps={{ sx: { width: { xs: '100%', sm: 400 }, display: 'flex', flexDirection: 'column', height: '100%' } }}
      >
        <Box sx={{ p: 2, borderBottom: `1px solid ${theme.palette.divider}`, display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
          <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
            {selectedConversation && (
              <IconButton size="small" onClick={() => setSelectedConversation(null)}>
                <ArrowBack />
              </IconButton>
            )}
            <Typography variant="h6">{t('chat.title')}</Typography>
            {totalUnread > 0 && (
              <Badge badgeContent={totalUnread} color="error" sx={{ ml: 1 }} />
            )}
          </Box>
          <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
            {!selectedConversation && (
              <IconButton size="small" onClick={() => setShowNewChat(true)}>
                <Add />
              </IconButton>
            )}
            <IconButton onClick={handleCloseChat}><Close /></IconButton>
          </Box>
        </Box>

        {!selectedConversation && showNewChat && (
          <Box sx={{ p: 2 }}>
            <TextField
              fullWidth
              size="small"
              placeholder={t('common.search') + '...'}
              value={searchUser}
              onChange={(e) => setSearchUser(e.target.value)}
              InputProps={{
                startAdornment: <InputAdornment position="start"><Search /></InputAdornment>,
              }}
            />
          </Box>
        )}

        {!selectedConversation ? (
          showNewChat ? (
            <List sx={{ flex: 1, overflow: 'auto', p: 0 }}>
              {filteredUsers.map((user: any) => (
                <ListItem 
                  key={user.id} 
                  onClick={() => handleStartChat(user.id, user.name)}
                  sx={{ 
                    cursor: 'pointer',
                    '&:hover': { bgcolor: alpha(theme.palette.primary.main, 0.1) }
                  }}
                >
                  <ListItemAvatar>
                    <Avatar>{user.name[0]}</Avatar>
                  </ListItemAvatar>
                  <ListItemText 
                    primary={user.name}
                  />
                </ListItem>
              ))}
              {filteredUsers.length === 0 && (
                <Box sx={{ p: 4, textAlign: 'center' }}>
                  <Typography color="text.secondary">{t('common.noData')}</Typography>
                </Box>
              )}
            </List>
          ) : (
            <List sx={{ flex: 1, overflow: 'auto', p: 0 }}>
              {conversations?.map((conv) => (
                <ListItem 
                  key={conv.id} 
                  onClick={() => handleSelectConversation(conv)}
                  sx={{ 
                    cursor: 'pointer',
                    bgcolor: conv.unreadCount > 0 ? alpha(theme.palette.primary.main, 0.05) : 'transparent',
                    '&:hover': { bgcolor: alpha(theme.palette.primary.main, 0.1) }
                  }}
                >
                  <ListItemAvatar>
                    <Badge 
                      overlap="circular" 
                      anchorOrigin={{ vertical: 'bottom', horizontal: 'right' }}
                      badgeContent={conv.participants.find(p => p.isOnline)?.userId ? (
                        <Box sx={{ width: 12, height: 12, bgcolor: '#4caf50', borderRadius: '50%', border: '2px solid white' }} />
                      ) : null}
                    >
                      <Avatar>{getConversationName(conv)[0]}</Avatar>
                    </Badge>
                  </ListItemAvatar>
                  <ListItemText 
                    primary={getConversationName(conv)}
                    secondary={conv.lastMessage?.content?.substring(0, 30)}
                    secondaryTypographyProps={{ noWrap: true }}
                  />
                  {conv.unreadCount > 0 && (
                    <Box sx={{ bgcolor: 'primary.main', color: 'white', borderRadius: 10, px: 1, py: 0.5, fontSize: 12 }}>
                      {conv.unreadCount}
                    </Box>
                  )}
                </ListItem>
              ))}
              
              {(!conversations || conversations.length === 0) && (
                <Box sx={{ p: 4, textAlign: 'center' }}>
                  <Typography color="text.secondary">{t('chat.noConversations')}</Typography>
                  <Button 
                    variant="contained" 
                    startIcon={<Add />} 
                    onClick={() => setShowNewChat(true)}
                    sx={{ mt: 2 }}
                  >
                    {t('chat.startConversation')}
                  </Button>
                </Box>
              )}
            </List>
          )
        ) : (
          <Box sx={{ flex: 1, display: 'flex', flexDirection: 'column' }}>
            <Box sx={{ p: 2, borderBottom: `1px solid ${theme.palette.divider}`, display: 'flex', alignItems: 'center', gap: 1 }}>
              <Avatar sx={{ width: 32, height: 32, fontSize: 14 }}>
                {getConversationName(selectedConversation)[0]}
              </Avatar>
              <Typography variant="subtitle1">{getConversationName(selectedConversation)}</Typography>
            </Box>

            <Box sx={{ flex: 1, overflow: 'auto', p: 2, display: 'flex', flexDirection: 'column', gap: 1 }}>
              {messages?.slice().reverse().map((msg) => {
                const isMe = msg.senderId === currentUser?.id;
                return (
                  <Box
                    key={msg.id}
                    sx={{
                      display: 'flex',
                      justifyContent: isMe ? 'flex-end' : 'flex-start'
                    }}
                  >
                    <Paper
                      sx={{
                        p: 1.5,
                        maxWidth: '80%',
                        bgcolor: isMe ? 'primary.main' : alpha(theme.palette.grey[500], 0.1),
                        color: isMe ? 'white' : 'text.primary',
                        borderRadius: 2
                      }}
                    >
                      <Typography variant="body2">{msg.content}</Typography>
                      <Typography variant="caption" sx={{ opacity: 0.7, display: 'block', textAlign: isMe ? 'right' : 'left' }}>
                        {new Date(msg.createdAt).toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' })}
                      </Typography>
                    </Paper>
                  </Box>
                );
              })}
              <div ref={messagesEndRef} />
            </Box>

            <Box sx={{ p: 2, borderTop: `1px solid ${theme.palette.divider}`, display: 'flex', gap: 1, alignItems: 'center' }}>
              <IconButton onClick={(e) => setEmojiAnchor(e.currentTarget)}>
                <EmojiEmotions />
              </IconButton>
              <Menu
                anchorEl={emojiAnchor}
                open={Boolean(emojiAnchor)}
                onClose={() => setEmojiAnchor(null)}
                anchorOrigin={{ vertical: 'top', horizontal: 'left' }}
                transformOrigin={{ vertical: 'bottom', horizontal: 'left' }}
              >
                <Box sx={{ display: 'grid', gridTemplateColumns: 'repeat(8, 1fr)', gap: 0.5, p: 1 }}>
                  {emojis.map((emoji) => (
                    <Button
                      key={emoji}
                      onClick={() => { setMessage(message + emoji); setEmojiAnchor(null); }}
                      sx={{ minWidth: 'auto', p: 0.5, fontSize: '1.2rem' }}
                    >
                      {emoji}
                    </Button>
                  ))}
                </Box>
              </Menu>
              <TextField
                fullWidth
                size="small"
                placeholder={t('chat.messagePlaceholder')}
                value={message}
                onChange={(e) => setMessage(e.target.value)}
                onKeyPress={(e) => e.key === 'Enter' && handleSendMessage()}
              />
              <IconButton color="primary" onClick={handleSendMessage} disabled={!message.trim()}>
                <Send />
              </IconButton>
            </Box>
          </Box>
        )}
      </Drawer>

      <Snackbar
        open={notificationOpen}
        autoHideDuration={3000}
        onClose={() => setNotificationOpen(false)}
        anchorOrigin={{ vertical: 'top', horizontal: 'center' }}
        sx={{ mt: 8 }}
      >
        <Alert severity="success" onClose={() => setNotificationOpen(false)} sx={{ width: '100%' }}>
          {notificationMsg}
        </Alert>
      </Snackbar>
    </>
  );
}
