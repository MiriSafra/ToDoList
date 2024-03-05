
import axios from 'axios';

// הגדרת הכתובת ה־API כ־default
axios.defaults.baseURL = "http://localhost:5168";

// הוספת Interceptor לתפיסת שגיאות ב־response ורישום ללוג
axios.interceptors.response.use(
  function (response) {
    console.log("succeded");
    return response;
  },
  error => {
    console.error('Request failed:', error);
    return Promise.reject(error);
  }
);

const TaskService = {
  getTasks: async () => {
    try {
      const result = await axios.get("/items");
      return result.data;
    } catch (error) {
      console.error('Error getting tasks:', error);
      throw error;
    }
  },

  addTask: async (name) => {
    try {
      const response = await axios.post("/items", { name });
      return response.data;
    } catch (error) {
      console.error('Error adding task:', error);
      throw error;
    }
  },

  setCompleted: async (id, isComplete) => {
    try {
      const response = await axios.put(`/items/${id}`, { isComplete });
      return response.data;
    } catch (error) {
      console.error('Error setting task completion status:', error);
      throw error;
    }
  },

  deleteTask: async (id) => {
    try {
      await axios.delete(`/items/${id}`);
      console.log('deleteTask', id);
    } catch (error) {
      console.error('Error deleting task:', error);
      throw error;
    }
  },
};
export default TaskService;
