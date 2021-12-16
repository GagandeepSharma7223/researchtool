﻿using chapterone.data.interfaces;
using chapterone.data.models;
using chapterone.services.interfaces;
using chapterone.web.viewmodels;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace chapterone.web.controllers
{
    /// <summary>
    /// Timeline API
    /// </summary>
    public class TimelineController : Controller
    {
        private const int TIMELINE_QUERY_PAGESIZE = 20;

        private readonly IDatabaseRepository<Message> _timelineRepo;
        private readonly IEventLogger _logger;

        /// <summary>
        /// Constructor
        /// </summary>
        public TimelineController(IDatabaseRepository<Message> timelineRepo, IEventLogger logger)
        {
            _timelineRepo = timelineRepo;
            _logger = logger;
        }


        [HttpGet("")]
        [HttpGet("/timeline")]
        public async Task<IActionResult> GetTimelineView([FromQuery] int page = 0)
        {
            try
            {
                var timeline = await _timelineRepo.QueryAsync(x => true, x => x.Created, isAscending: false, pageNumber: page, pageSize: TIMELINE_QUERY_PAGESIZE);
                var maxCount = await _timelineRepo.CountAsync(x => true);

                var vm = new TimelineViewModel()
                {
                    Entries = timeline.OrderByDescending(x => x.Created.LocalDateTime),
                    CurrentPage = page,
                    MaxPages = (int)maxCount / 20
                };

                return View("~/views/Timeline.cshtml", vm);
            }
            catch (Exception ex)
            {
                _logger.LogException(ex);
                return NotFound();
            }
        }
    }
}
